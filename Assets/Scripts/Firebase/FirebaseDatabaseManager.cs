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
                    if (Application.isPlaying) DontDestroyOnLoad(go);
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
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
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
                isBookUnlocked = false,
                reviewedBookTopics = new List<int>(),
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
            if (data.reviewedBookTopics != null)
            {
                data.reviewedBookTopics.RemoveAll(t => t < 1 || t > 12);
            }
            else
            {
                data.reviewedBookTopics = new List<int>();
            }

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
            if (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsGuest)
            {
                var guestData = CurrentPlayerData ?? BuildPlayerDataFromLocalState();
                onComplete?.Invoke(guestData != null, guestData);
                return;
            }

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

        /// <summary>
        /// Checks whether a cloud save already exists in Firebase for the specified user ID without modifying local state.
        /// Returns (requestSuccess, hasExistingProgress, data).
        /// </summary>
        public void CheckCloudSaveExists(string userId, string token, Action<bool, bool, PlayerData> onComplete)
        {
            if (string.IsNullOrEmpty(userId))
            {
                onComplete?.Invoke(false, false, null);
                return;
            }

            string url = $"{FirebaseConfig.DatabaseUrl}/users/{userId}.json";
            if (!string.IsNullOrEmpty(token) && token != "mock_editor_token")
            {
                url += $"?auth={token}";
            }

            StartCoroutine(GetCoroutine(url, (success, response) =>
            {
                if (!success)
                {
                    Debug.LogError($"[FirebaseDatabaseManager] CheckCloudSaveExists error: {response}");
                    onComplete?.Invoke(false, false, null);
                    return;
                }

                if (string.IsNullOrEmpty(response) || response == "null")
                {
                    // No existing save in Firebase
                    onComplete?.Invoke(true, false, null);
                    return;
                }

                try
                {
                    var data = JsonUtility.FromJson<PlayerData>(response);
                    if (data != null)
                    {
                        bool hasAnyRoomProgress = false;
                        if (data.roomProgress != null)
                        {
                            foreach (var rp in data.roomProgress)
                            {
                                if (rp != null && (rp.battleCompleted || rp.artifactRetrieved || rp.returnedToRealWorld || rp.restoredPedestalsCount > 0 || rp.retrievedCount > 0))
                                {
                                    hasAnyRoomProgress = true;
                                    break;
                                }
                            }
                        }

                        bool hasProgress = (data.hasSelectedCharacter && !string.IsNullOrEmpty(data.selectedCharacter)) ||
                                           hasAnyRoomProgress ||
                                           (data.reviewedBookTopics != null && data.reviewedBookTopics.Count > 0) ||
                                           data.hasCompletedTutorial ||
                                           data.isRoom1BattleCompleted ||
                                           (data.inventoryArtifacts != null && data.inventoryArtifacts.Count > 0);

                        onComplete?.Invoke(true, hasProgress, data);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[FirebaseDatabaseManager] CheckCloudSaveExists parse error: {ex.Message}");
                }

                onComplete?.Invoke(true, false, null);
            }));
        }

        public PlayerData BuildPlayerDataFromLocalState()
        {
            bool hasChar = PlayerPrefs.GetInt("HasSelectedCharacter", 0) == 1;
            string chosenChar = PlayerPrefs.GetString("SelectedCharacter", "");
            if (!hasChar && string.IsNullOrEmpty(chosenChar)) return null;

            var data = CreateFreshPlayerData();
            data.username = PlayerPrefs.GetString("PlayerUsername", "Guest");
            data.selectedCharacter = chosenChar;
            data.characterName = PlayerPrefs.GetString("CustomCharacterName", chosenChar == "Girl" ? "Darla" : "Darrel");
            data.hasSelectedCharacter = true;
            data.currentScene = PlayerPrefs.GetString("LastActiveScene", "Lobby");
            data.returnSceneName = PlayerPrefs.GetString("RoomTransition_ReturnScene", "");
            data.returnDoorName = PlayerPrefs.GetString("RoomTransition_ReturnDoor", "");
            data.returnPositionX = PlayerPrefs.GetFloat("RoomTransition_ReturnPosX", 0f);
            data.returnPositionY = PlayerPrefs.GetFloat("RoomTransition_ReturnPosY", 0f);
            data.hasReturnPosition = PlayerPrefs.GetInt("RoomTransition_HasReturnPos", 0) == 1;
            data.isPortalUnlocked = GameProgression.IsPortalUnlocked;
            data.isBookUnlocked = GameProgression.IsBookUnlocked;
            data.reviewedBookTopics = new List<int>();
            for (int t = 1; t <= 12; t++)
            {
                if (GameProgression.IsBookTopicReviewed(t))
                {
                    data.reviewedBookTopics.Add(t);
                }
            }
            data.hasCompletedTutorial = GameProgression.HasCompletedTutorial;
            data.isRoom1BattleCompleted = GameProgression.IsRoom1BattleCompleted;

            // Load any saved room progress
            for (int r = 1; r <= 12; r++)
            {
                var entry = data.roomProgress.Find(p => p.roomNumber == r);
                if (entry != null)
                {
                    entry.battleCompleted = GameProgression.IsRoomBattleCompleted(r);
                    entry.artifactRetrieved = GameProgression.HasRetrievedRoomArtifact(r);
                    entry.returnedToRealWorld = GameProgression.HasReturnedRoomArtifactsToRealWorld(r);
                    entry.retrievedCount = GameProgression.GetRoomRetrievedCount(r);
                    entry.restoredPedestalsCount = GameProgression.GetPedestalsRestoredCount(r);
                    entry.plaquesList = PlayerPrefs.GetString($"GameProgression_Room{r}_PlaquesList", "");

                    string listKey = $"GameProgression_Room{r}_RestoredArtifactsList";
                    string currentList = PlayerPrefs.GetString(listKey, "");
                    if (!string.IsNullOrEmpty(currentList))
                    {
                        entry.restoredPedestalArtifacts = new List<string>(currentList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                }
            }

            CurrentPlayerData = data;
            return data;
        }

        public void ApplyDataToGame(PlayerData data, bool clearExisting = true)
        {
            if (data == null) return;

            // 1. Capture current Google authentication credentials before clearing local data
            bool isGoogleUser = (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsGoogleUser) ||
                                (PlayerPrefs.GetString("AuthType", "") == "google");
            string authUid = FirebaseAuthManager.Instance != null && !FirebaseAuthManager.Instance.IsGuest
                ? FirebaseAuthManager.Instance.UserId
                : PlayerPrefs.GetString("AuthUserId", "");
            string authEmail = !string.IsNullOrEmpty(data.email) && !data.email.Contains("@pamana.game")
                ? data.email
                : (FirebaseAuthManager.Instance != null ? FirebaseAuthManager.Instance.Email : PlayerPrefs.GetString("AuthEmail", ""));
            string authUsername = !string.IsNullOrEmpty(data.username) && data.username != "Guest"
                ? data.username
                : (FirebaseAuthManager.Instance != null ? FirebaseAuthManager.Instance.Username : PlayerPrefs.GetString("PlayerUsername", ""));
            string authToken = FirebaseAuthManager.Instance != null ? FirebaseAuthManager.Instance.IdToken : PlayerPrefs.GetString("AuthIdToken", "");

            // 2. Clear all local game state if requested (for clean cloud load / fresh account isolation)
            if (clearExisting)
            {
                ClearAllLocalGameData();
            }

            CurrentPlayerData = data;

            // 3. Restore Google authentication session in PlayerPrefs and in-memory manager
            if (isGoogleUser && !string.IsNullOrEmpty(authUid))
            {
                if (FirebaseAuthManager.Instance != null)
                {
                    FirebaseAuthManager.Instance.SetGoogleSession(authUid, authEmail, authUsername, authToken);
                }
                else
                {
                    PlayerPrefs.SetString("AuthType", "google");
                    PlayerPrefs.SetString("AuthUserId", authUid);
                    PlayerPrefs.SetString("AuthEmail", authEmail);
                    PlayerPrefs.SetString("PlayerUsername", authUsername);
                    if (!string.IsNullOrEmpty(authToken)) PlayerPrefs.SetString("AuthIdToken", authToken);
                    PlayerPrefs.SetInt("HasGuestSave", 0);
                    PlayerPrefs.DeleteKey("GuestUserId");
                }
            }

            if (!string.IsNullOrEmpty(data.username))
            {
                PlayerPrefs.SetString("PlayerUsername", data.username);
            }

            if (!string.IsNullOrEmpty(data.characterName) && data.characterName != "Guest")
            {
                PlayerPrefs.SetString("CustomCharacterName", data.characterName);
            }
            else if (!string.IsNullOrEmpty(data.username) && data.username != "Guest")
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
            GameProgression.SetBookUnlocked(data.isBookUnlocked);
            if (data.reviewedBookTopics != null)
            {
                foreach (int topic in data.reviewedBookTopics)
                {
                    if (topic >= 1 && topic <= 12)
                    {
                        GameProgression.SetBookTopicReviewed(topic, true);
                    }
                }
            }

            if (!string.IsNullOrEmpty(data.returnSceneName))
            {
                RoomTransitionManager.ReturnSceneName = data.returnSceneName;
                RoomTransitionManager.ReturnDoorName = data.returnDoorName;
                RoomTransitionManager.ReturnPosition = new Vector2(data.returnPositionX, data.returnPositionY);
                RoomTransitionManager.HasReturnPosition = data.hasReturnPosition;
                RoomTransitionManager.SaveToPrefs();
            }

            // 4. Restore all 12 rooms progress
            bool room1RestoredOrBattleDone = data.isRoom1BattleCompleted;
            if (data.roomProgress != null)
            {
                foreach (var r in data.roomProgress)
                {
                    if (r == null || r.roomNumber <= 0) continue;
                    int num = r.roomNumber;

                    int restoredCount = r.restoredPedestalsCount;
                    int retrievedCount = r.retrievedCount;
                    bool battleCompleted = r.battleCompleted;
                    bool artifactRetrieved = r.artifactRetrieved;
                    bool returnedToRealWorld = r.returnedToRealWorld;

                    if (restoredCount >= 5)
                    {
                        battleCompleted = true;
                        artifactRetrieved = true;
                        returnedToRealWorld = true;
                        if (retrievedCount < 5) retrievedCount = 5;
                    }
                    else if (restoredCount > 0)
                    {
                        returnedToRealWorld = true;
                        if (retrievedCount < restoredCount) retrievedCount = restoredCount;
                    }

                    if (num == 1 && (battleCompleted || restoredCount > 0 || returnedToRealWorld))
                    {
                        room1RestoredOrBattleDone = true;
                    }

                    // Write counts to PlayerPrefs first so properties dependent on count evaluate correctly
                    PlayerPrefs.SetInt($"GameProgression_Room{num}_RestoredPedestalsCount", restoredCount);
                    PlayerPrefs.SetInt($"GameProgression_Room{num}_RetrievedCount", retrievedCount);

                    PlayerPrefs.SetInt($"GameProgression_Room{num}BattleCompleted", battleCompleted ? 1 : 0);
                    PlayerPrefs.SetInt($"GameProgression_Room{num}ArtifactRetrieved", artifactRetrieved ? 1 : 0);
                    PlayerPrefs.SetInt($"GameProgression_Room{num}ArtifactReturnedToRealWorld", returnedToRealWorld ? 1 : 0);

                    if (num == 1)
                    {
                        GameProgression.IsRoom1BattleCompleted = battleCompleted;
                        GameProgression.HasRetrievedRoom1Artifact = artifactRetrieved;
                        GameProgression.HasReturnedRoom1ArtifactToRealWorld = returnedToRealWorld;
                    }

                    // Plaques list
                    string plaques = r.plaquesList;
                    if (string.IsNullOrEmpty(plaques) && num == 1 && restoredCount > 0)
                    {
                        plaques = "Wire-rimmed Spectacles;University Wax Seal Stamp;Travel Notebook;Dip Pen Nib;Gold Locket Pendant";
                    }
                    PlayerPrefs.SetString($"GameProgression_Room{num}_PlaquesList", plaques ?? "");

                    // Individual pedestal artifacts
                    var restoredArtifacts = r.restoredPedestalArtifacts ?? new List<string>();
                    if (num == 1 && restoredCount >= 5 && restoredArtifacts.Count < 5)
                    {
                        restoredArtifacts = new List<string> {
                            "Wire-rimmed Spectacles",
                            "University Wax Seal Stamp",
                            "Travel Notebook",
                            "Dip Pen Nib",
                            "Gold Locket Pendant"
                        };
                    }

                    if (restoredArtifacts.Count > 0)
                    {
                        foreach (var art in restoredArtifacts)
                        {
                            if (string.IsNullOrEmpty(art)) continue;
                            string clean = art.Replace(" ", "_").Replace("'", "");
                            string stripped = art.Replace(" ", "").Replace("_", "").Replace("-", "").Replace("'", "");
                            PlayerPrefs.SetInt($"GameProgression_Room{num}_Pedestal_{clean}_Restored", 1);
                            PlayerPrefs.SetInt($"GameProgression_Room{num}_Pedestal_{stripped}_Restored", 1);
                        }
                        PlayerPrefs.SetString($"GameProgression_Room{num}_RestoredArtifactsList", string.Join(";", restoredArtifacts));
                    }
                }
            }

            // 5. Restore Tutorial state
            bool tutorialCompleted = data.hasCompletedTutorial || room1RestoredOrBattleDone;
            GameProgression.HasCompletedTutorial = tutorialCompleted;
            PlayerPrefs.SetInt("GameProgression_TutorialCompleted", tutorialCompleted ? 1 : 0);
            if (tutorialCompleted)
            {
                PlayerPrefs.SetInt("GameProgression_TutorialPhase", 7);
                GameProgression.SetBookUnlocked(true);
                GameProgression.UnlockPortal();
                if (GameTutorialGuide.Instance != null)
                {
                    GameTutorialGuide.Instance.DismissAndDestroy();
                }
                GameTutorialGuide.PurgeAllTutorialObjects();
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
            Debug.Log($"[FirebaseDatabaseManager] Applied player data to game for '{data.username}' (hasSelectedCharacter: {data.hasSelectedCharacter}, char: '{data.selectedCharacter}', tutCompleted: {tutorialCompleted})");
        }

        public void SyncFromGameAndSave(Action<bool, string> onComplete = null)
        {
            var data = CurrentPlayerData ?? new PlayerData();
            data.username = PlayerPrefs.GetString("PlayerUsername", data.username);
            data.selectedCharacter = PlayerPrefs.GetString("SelectedCharacter", data.selectedCharacter);
            data.characterName = PlayerPrefs.GetString("CustomCharacterName", data.characterName);
            data.hasSelectedCharacter = PlayerPrefs.GetInt("HasSelectedCharacter", 0) == 1 || data.hasSelectedCharacter;

            // Preserve authentic Google account identity if Google user
            if (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsGoogleUser)
            {
                if (!string.IsNullOrEmpty(FirebaseAuthManager.Instance.Email))
                {
                    data.email = FirebaseAuthManager.Instance.Email;
                }
                if (!string.IsNullOrEmpty(FirebaseAuthManager.Instance.Username))
                {
                    data.username = FirebaseAuthManager.Instance.Username;
                }
            }

            data.masterVolume = AudioSettingsManager.MasterVolume;
            data.sfxVolume = AudioSettingsManager.SFXVolume;
            data.bgmVolume = AudioSettingsManager.BGMVolume;
            data.isPortalUnlocked = GameProgression.IsPortalUnlocked;
            data.isBookUnlocked = GameProgression.IsBookUnlocked;
            data.reviewedBookTopics = new List<int>();
            for (int t = 1; t <= 12; t++)
            {
                if (GameProgression.IsBookTopicReviewed(t))
                {
                    data.reviewedBookTopics.Add(t);
                }
            }

            int room1RestoredCount = GameProgression.GetPedestalsRestoredCount(1);
            data.isRoom1BattleCompleted = GameProgression.IsRoom1BattleCompleted || room1RestoredCount >= 5;
            data.hasCompletedTutorial = GameProgression.HasCompletedTutorial || room1RestoredCount > 0 || data.isRoom1BattleCompleted;

            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeScene != "MainMenu" && activeScene != "CharacterSelect")
            {
                data.currentScene = activeScene;
            }

            // Ensure return hallway/door information is synced
            RoomTransitionManager.EnsureRoomReturnInfo(activeScene);
            data.returnSceneName = RoomTransitionManager.ReturnSceneName;
            data.returnDoorName = RoomTransitionManager.ReturnDoorName;
            data.returnPositionX = RoomTransitionManager.ReturnPosition.x;
            data.returnPositionY = RoomTransitionManager.ReturnPosition.y;
            data.hasReturnPosition = RoomTransitionManager.HasReturnPosition;

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

                int restoredCount = GameProgression.GetPedestalsRestoredCount(r);
                if (restoredList.Count > restoredCount) restoredCount = restoredList.Count;

                int retrievedCount = GameProgression.GetRoomRetrievedCount(r);
                if (restoredCount > retrievedCount) retrievedCount = restoredCount;

                bool battleCompleted = GameProgression.IsRoomBattleCompleted(r);
                bool artifactRetrieved = GameProgression.HasRetrievedRoomArtifact(r);
                bool returnedToRealWorld = GameProgression.HasReturnedRoomArtifactsToRealWorld(r) || (restoredCount > 0);

                if (restoredCount >= 5)
                {
                    battleCompleted = true;
                    artifactRetrieved = true;
                    returnedToRealWorld = true;
                    if (retrievedCount < 5) retrievedCount = 5;
                }

                if (r == 1 && restoredCount >= 5 && restoredList.Count < 5)
                {
                    restoredList = new List<string> {
                        "Wire-rimmed Spectacles",
                        "University Wax Seal Stamp",
                        "Travel Notebook",
                        "Dip Pen Nib",
                        "Gold Locket Pendant"
                    };
                }

                string plaques = PlayerPrefs.GetString($"GameProgression_Room{r}_PlaquesList", "");
                if (string.IsNullOrEmpty(plaques) && r == 1 && restoredCount > 0)
                {
                    plaques = "Wire-rimmed Spectacles;University Wax Seal Stamp;Travel Notebook;Dip Pen Nib;Gold Locket Pendant";
                }

                var entry = new RoomProgressData
                {
                    roomNumber = r,
                    battleCompleted = battleCompleted,
                    artifactRetrieved = artifactRetrieved,
                    returnedToRealWorld = returnedToRealWorld,
                    retrievedCount = retrievedCount,
                    restoredPedestalsCount = restoredCount,
                    plaquesList = plaques,
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
