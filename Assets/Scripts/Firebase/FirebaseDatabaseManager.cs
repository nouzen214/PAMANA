using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TopDownGame
{
    public class FirebaseDatabaseManager : MonoBehaviour
    {
        private static FirebaseDatabaseManager _instance;
        public static FirebaseDatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("FirebaseDatabaseManager");
                    _instance = go.AddComponent<FirebaseDatabaseManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public PlayerData CurrentPlayerData { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Clears all in-memory and PlayerPrefs game data so that a new or switched account
        /// does NOT inherit any progress, character selection, or inventory from the previous account.
        /// </summary>
        public void ClearAllLocalGameData()
        {
            Debug.Log("[FirebaseDatabaseManager] Resetting all local state for clean account isolation...");

            // 1. Reset all progression in GameProgression (all rooms, plaques, battles, pedestals)
            GameProgression.ResetProgression();

            // 2. Clear player inventory silently so AutoSave is not triggered
            PlayerInventory.ClearSilently();

            // 3. Reset door/hallway return position coordinates
            RoomTransitionManager.Reset();

            // 4. Preserve audio volume settings
            float mVol = AudioSettingsManager.MasterVolume;
            float sVol = AudioSettingsManager.SFXVolume;
            float bVol = AudioSettingsManager.BGMVolume;

            // 5. WIPE ALL PLAYERPREFS COMPLETELY to ensure no stale pedestal or plaque keys remain
            PlayerPrefs.DeleteAll();

            // 6. Restore audio volume
            AudioSettingsManager.SetMaster(mVol);
            AudioSettingsManager.SetSFX(sVol);
            AudioSettingsManager.SetBGM(bVol);
            AudioSettingsManager.Save();

            CurrentPlayerData = null;
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Creates a brand-new player profile with zero progress and no character selected.
        /// Never reads from existing PlayerPrefs to prevent cross-account contamination.
        /// </summary>
        public PlayerData CreateFreshPlayerData()
        {
            string customName = PlayerPrefs.GetString("CustomCharacterName", "");
            var data = new PlayerData
            {
                username = FirebaseAuthManager.Instance.Username,
                email = FirebaseAuthManager.Instance.Email,
                selectedCharacter = "",
                characterName = customName,
                hasSelectedCharacter = false,
                currentScene = "Lobby",
                coins = 0,
                masterVolume = 1f,
                sfxVolume = 1f,
                bgmVolume = 1f,
                isPortalUnlocked = false,
                hasCompletedTutorial = false,
                isRoom1BattleCompleted = false,
                roomProgress = new List<RoomProgressData>(),
                inventoryItems = new List<SavedInventoryItem>(),
                inventoryArtifacts = new List<string>()
            };

            for (int r = 1; r <= 12; r++)
            {
                data.roomProgress.Add(new RoomProgressData
                {
                    roomNumber = r,
                    battleCompleted = false,
                    artifactRetrieved = false,
                    returnedToRealWorld = false,
                    retrievedCount = 0,
                    restoredPedestalsCount = 0,
                    plaquesList = "",
                    restoredPedestalArtifacts = new List<string>()
                });
            }

            return data;
        }

        /// <summary>
        /// Validates and sanitizes PlayerData fields before serialization and cloud dispatch
        /// to prevent out-of-bounds progress injection, negative values, or corrupted states.
        /// </summary>
        public static void ValidatePlayerData(PlayerData data)
        {
            if (data == null) return;

            // 1. Coins and economy boundaries
            data.coins = Mathf.Max(0, data.coins);

            // 2. Character name sanity
            if (!string.IsNullOrEmpty(data.characterName) && data.characterName.Length > 20)
            {
                data.characterName = data.characterName.Substring(0, 20).Trim();
            }

            // 3. Room progress data clamping and structure validation
            if (data.roomProgress != null)
            {
                foreach (var rp in data.roomProgress)
                {
                    if (rp == null) continue;
                    rp.roomNumber = Mathf.Clamp(rp.roomNumber, 1, 12);
                    rp.retrievedCount = Mathf.Clamp(rp.retrievedCount, 0, 5);
                    rp.restoredPedestalsCount = Mathf.Clamp(rp.restoredPedestalsCount, 0, 5);
                }
            }
        }

        public void SavePlayerData(PlayerData data, Action<bool, string> onComplete = null)
        {
            string userId = FirebaseAuthManager.Instance.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                onComplete?.Invoke(false, "User is not signed in.");
                return;
            }

            // Perform defensive validation on outgoing payload to prevent invalid scores or corrupted progression
            ValidatePlayerData(data);

            if (FirebaseAuthManager.Instance.IsGuest)
            {
                // Guest progress is in-memory / local session only and NOT saved to cloud
                data.lastLogin = DateTime.UtcNow.ToString("o");
                CurrentPlayerData = data;
                onComplete?.Invoke(true, "Local guest progress recorded.");
                return;
            }

            string token = FirebaseAuthManager.Instance.IdToken;
            string url = $"{FirebaseConfig.DatabaseUrl}/users/{userId}.json";
            if (!string.IsNullOrEmpty(token) && token != "mock_editor_token")
            {
                url += $"?auth={token}";
            }

            data.lastLogin = DateTime.UtcNow.ToString("o");
            CurrentPlayerData = data;

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString($"LocalPlayerDataBackup_{userId}", json);
            PlayerPrefs.Save();

            StartCoroutine(PutCoroutine(url, json, (success, response) =>
            {
                if (success)
                {
                    Debug.Log($"[FirebaseDatabaseManager] Saved player data successfully for user: {userId}");
                    onComplete?.Invoke(true, "Data saved successfully!");
                }
                else
                {
                    Debug.LogWarning($"[FirebaseDatabaseManager] Save error: {response}");
                    onComplete?.Invoke(false, response);
                }
            }));
        }

        public void LoadPlayerData(Action<bool, PlayerData> onComplete)
        {
            string userId = FirebaseAuthManager.Instance.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                onComplete?.Invoke(false, null);
                return;
            }

            string token = FirebaseAuthManager.Instance.IdToken;
            string url = $"{FirebaseConfig.DatabaseUrl}/users/{userId}.json";
            if (!string.IsNullOrEmpty(token) && token != "mock_editor_token")
            {
                url += $"?auth={token}";
            }

            StartCoroutine(GetCoroutine(url, (success, response) =>
            {
                if (success && !string.IsNullOrEmpty(response) && response != "null")
                {
                    try
                    {
                        var data = JsonUtility.FromJson<PlayerData>(response);
                        if (data != null && (!string.IsNullOrEmpty(data.username) || !string.IsNullOrEmpty(data.email)))
                        {
                            ApplyDataToGame(data);
                            Debug.Log($"[FirebaseDatabaseManager] Loaded existing player data for: {data.username} (hasSelectedCharacter: {data.hasSelectedCharacter})");
                            onComplete?.Invoke(true, data);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[FirebaseDatabaseManager] Parse error on load: {ex.Message}");
                    }
                }

                // If no existing data in Firebase (new account or first Google sign-in):
                Debug.Log($"[FirebaseDatabaseManager] No cloud save found for user '{userId}'. Creating brand new account data with 0 progress.");
                ClearAllLocalGameData();
                var freshData = CreateFreshPlayerData();
                CurrentPlayerData = freshData;
                SavePlayerData(freshData, null);
                onComplete?.Invoke(true, freshData);
            }));
        }

        public void ApplyDataToGame(PlayerData data)
        {
            // Clear all local game state first to avoid cross-account contamination
            ClearAllLocalGameData();

            if (data == null) return;
            CurrentPlayerData = data;

            if (!string.IsNullOrEmpty(data.username))
            {
                PlayerPrefs.SetString("PlayerUsername", data.username);
            }

            if (!string.IsNullOrEmpty(data.characterName))
            {
                PlayerPrefs.SetString("CustomCharacterName", data.characterName);
            }
            else if (!string.IsNullOrEmpty(data.username))
            {
                PlayerPrefs.SetString("CustomCharacterName", data.username);
            }

            if (!string.IsNullOrEmpty(data.selectedCharacter))
            {
                PlayerPrefs.SetString("SelectedCharacter", data.selectedCharacter);
            }

            PlayerPrefs.SetInt("HasSelectedCharacter", data.hasSelectedCharacter ? 1 : 0);

            AudioSettingsManager.SetMaster(data.masterVolume);
            AudioSettingsManager.SetSFX(data.sfxVolume);
            AudioSettingsManager.SetBGM(data.bgmVolume);
            AudioSettingsManager.Save();

            GameProgression.IsPortalUnlocked = data.isPortalUnlocked;
            GameProgression.HasCompletedTutorial = data.hasCompletedTutorial;
            GameProgression.IsRoom1BattleCompleted = data.isRoom1BattleCompleted;

            // Restore all 12 rooms progress
            if (data.roomProgress != null)
            {
                foreach (var r in data.roomProgress)
                {
                    if (r == null || r.roomNumber <= 0) continue;
                    int num = r.roomNumber;

                    PlayerPrefs.SetInt($"GameProgression_Room{num}BattleCompleted", r.battleCompleted ? 1 : 0);
                    if (num == 1)
                    {
                        GameProgression.IsRoom1BattleCompleted = r.battleCompleted;
                        GameProgression.HasRetrievedRoom1Artifact = r.artifactRetrieved;
                        GameProgression.HasReturnedRoom1ArtifactToRealWorld = r.returnedToRealWorld;
                    }

                    PlayerPrefs.SetInt($"GameProgression_Room{num}ArtifactRetrieved", r.artifactRetrieved ? 1 : 0);
                    PlayerPrefs.SetInt($"GameProgression_Room{num}ArtifactReturnedToRealWorld", r.returnedToRealWorld ? 1 : 0);
                    PlayerPrefs.SetInt($"GameProgression_Room{num}_RetrievedCount", r.retrievedCount);
                    PlayerPrefs.SetInt($"GameProgression_Room{num}_RestoredPedestalsCount", r.restoredPedestalsCount);
                    PlayerPrefs.SetString($"GameProgression_Room{num}_PlaquesList", r.plaquesList ?? "");

                    if (r.restoredPedestalArtifacts != null && r.restoredPedestalArtifacts.Count > 0)
                    {
                        foreach (var art in r.restoredPedestalArtifacts)
                        {
                            if (string.IsNullOrEmpty(art)) continue;
                            string clean = art.Replace(" ", "_").Replace("'", "");
                            PlayerPrefs.SetInt($"GameProgression_Room{num}_Pedestal_{clean}_Restored", 1);
                        }
                        PlayerPrefs.SetString($"GameProgression_Room{num}_RestoredArtifactsList", string.Join(";", r.restoredPedestalArtifacts));
                    }
                }
            }

            // Restore Player Inventory
            PlayerInventory.Clear();
            if (data.inventoryItems != null && data.inventoryItems.Count > 0)
            {
                foreach (var item in data.inventoryItems)
                {
                    if (item == null || string.IsNullOrEmpty(item.id)) continue;
                    Sprite icon = null;
                    if (!string.IsNullOrEmpty(item.spriteName))
                    {
                        icon = Resources.Load<Sprite>(item.spriteName);
                    }
                    PlayerInventory.AddArtifact(item.id, item.name, icon, item.description);
                }
            }

            PlayerPrefs.Save();
            Debug.Log($"[FirebaseDatabaseManager] Applied player data to game for '{data.username}' (hasSelectedCharacter: {data.hasSelectedCharacter}, char: '{data.selectedCharacter}')");
        }

        public void SyncFromGameAndSave(Action<bool, string> onComplete = null)
        {
            var data = CurrentPlayerData ?? new PlayerData();
            data.username = PlayerPrefs.GetString("PlayerUsername", data.username);
            data.selectedCharacter = PlayerPrefs.GetString("SelectedCharacter", data.selectedCharacter);
            data.characterName = PlayerPrefs.GetString("CustomCharacterName", data.characterName);
            data.hasSelectedCharacter = PlayerPrefs.GetInt("HasSelectedCharacter", 0) == 1 || data.hasSelectedCharacter;
            data.masterVolume = AudioSettingsManager.MasterVolume;
            data.sfxVolume = AudioSettingsManager.SFXVolume;
            data.bgmVolume = AudioSettingsManager.BGMVolume;
            data.isPortalUnlocked = GameProgression.IsPortalUnlocked;
            data.hasCompletedTutorial = GameProgression.HasCompletedTutorial;
            data.isRoom1BattleCompleted = GameProgression.IsRoom1BattleCompleted;

            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeScene != "MainMenu" && activeScene != "CharacterSelect")
            {
                data.currentScene = activeScene;
            }

            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                data.positionX = playerObj.transform.position.x;
                data.positionY = playerObj.transform.position.y;
                var health = playerObj.GetComponent<Health>();
                if (health != null)
                {
                    data.currentHealth = health.CurrentHealth;
                    data.maxHealth = health.MaxHealth;
                }
            }

            // Sync Inventory Items
            if (data.inventoryItems == null) data.inventoryItems = new List<SavedInventoryItem>();
            if (data.inventoryArtifacts == null) data.inventoryArtifacts = new List<string>();
            data.inventoryItems.Clear();
            data.inventoryArtifacts.Clear();

            if (PlayerInventory.Items != null)
            {
                foreach (var item in PlayerInventory.Items)
                {
                    if (item != null && !string.IsNullOrEmpty(item.id))
                    {
                        data.inventoryArtifacts.Add(item.id);
                        data.inventoryItems.Add(new SavedInventoryItem
                        {
                            id = item.id,
                            name = item.name,
                            description = item.description,
                            spriteName = item.icon != null ? item.icon.name : ""
                        });
                    }
                }
            }

            // Sync all 12 Rooms Progression
            if (data.roomProgress == null) data.roomProgress = new List<RoomProgressData>();
            data.roomProgress.Clear();

            for (int r = 1; r <= 12; r++)
            {
                string listKey = $"GameProgression_Room{r}_RestoredArtifactsList";
                string currentList = PlayerPrefs.GetString(listKey, "");
                var restoredList = new List<string>();
                if (!string.IsNullOrEmpty(currentList))
                {
                    restoredList.AddRange(currentList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                }

                var entry = new RoomProgressData
                {
                    roomNumber = r,
                    battleCompleted = GameProgression.IsRoomBattleCompleted(r),
                    artifactRetrieved = GameProgression.HasRetrievedRoomArtifact(r),
                    returnedToRealWorld = GameProgression.HasReturnedRoomArtifactsToRealWorld(r),
                    retrievedCount = GameProgression.GetRoomRetrievedCount(r),
                    restoredPedestalsCount = GameProgression.GetPedestalsRestoredCount(r),
                    plaquesList = PlayerPrefs.GetString($"GameProgression_Room{r}_PlaquesList", ""),
                    restoredPedestalArtifacts = restoredList
                };
                data.roomProgress.Add(entry);
            }

            SavePlayerData(data, onComplete);
        }

        private IEnumerator PutCoroutine(string url, string jsonBody, Action<bool, string> callback)
        {
            using (var uwr = new UnityWebRequest(url, "PUT"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                uwr.SetRequestHeader("Content-Type", "application/json");

                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, uwr.downloadHandler.text);
                }
                else
                {
                    string err = uwr.downloadHandler != null ? uwr.downloadHandler.text : uwr.error;
                    callback?.Invoke(false, err);
                }
            }
        }

        private IEnumerator GetCoroutine(string url, Action<bool, string> callback)
        {
            using (var uwr = UnityWebRequest.Get(url))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, uwr.downloadHandler.text);
                }
                else
                {
                    string err = uwr.downloadHandler != null ? uwr.downloadHandler.text : uwr.error;
                    callback?.Invoke(false, err);
                }
            }
        }
    }
}
