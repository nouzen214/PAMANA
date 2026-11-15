using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopDownGame
{
    public class AutoSaveManager : MonoBehaviour
    {
        private static AutoSaveManager _instance;
        public static AutoSaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AutoSaveManager");
                    _instance = go.AddComponent<AutoSaveManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("AutoSave Settings")]
        [SerializeField] private float autoSaveInterval = 45f;
        [SerializeField] private float minSaveCooldown = 5f;

        private float _lastSaveTime = -999f;
        private bool _isSavingNow = false;
        private bool _hasPendingSave = false;
        private string _pendingSaveReason = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInit()
        {
            if (_instance == null)
            {
                var inst = Instance;
            }
        }

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

        private void Start()
        {
            // Subscribe to scene loads
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Subscribe to progression events
            GameProgression.OnRoom1BattleCompleted += () => TriggerAutoSave("Room 1 Battle Completed");
            GameProgression.OnPortalUnlocked += () => TriggerAutoSave("Portal Unlocked");
            GameProgression.OnRoom1ArtifactRetrieved += () => TriggerAutoSave("Artifact Retrieved");
            GameProgression.OnRoom1ArtifactReturned += () => TriggerAutoSave("Artifact Returned");
            GameProgression.OnPedestalRestored += (r, a) => TriggerAutoSave($"Pedestal Restored: Room {r} {a}");
            GameProgression.OnPlaqueRead += (r, c) => TriggerAutoSave($"Plaque Read: Room {r} ({c}/5)");
            GameProgression.OnBookUnlocked += () => TriggerAutoSave("Field Guide Unlocked");
            GameProgression.OnBookTopicReviewed += (t) => TriggerAutoSave($"Field Guide Topic {t} Reviewed");

            // Subscribe to inventory events
            PlayerInventory.OnInventoryChanged += () => TriggerAutoSave("Inventory Changed");

            // Start periodic autosave loop
            StartCoroutine(PeriodicAutoSaveCoroutine());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Don't autosave on main menu
            if (scene.name == "MainMenu" || scene.name == "CharacterSelect") return;

            // Wait a moment for scene objects to initialize before saving
            StartCoroutine(DelayedSaveCoroutine(2f, $"Entered {scene.name}"));
        }

        private IEnumerator DelayedSaveCoroutine(float delay, string reason)
        {
            yield return new WaitForSeconds(delay);
            TriggerAutoSave(reason);
        }

        private IEnumerator PeriodicAutoSaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoSaveInterval);

                string activeScene = SceneManager.GetActiveScene().name;
                if (activeScene != "MainMenu" && activeScene != "CharacterSelect")
                {
                    TriggerAutoSave("Periodic Timer");
                }
            }
        }

        public void TriggerAutoSave(string reason = "AutoSave")
        {
            // Never autosave in MainMenu or CharacterSelect
            string activeScene = SceneManager.GetActiveScene().name;
            if (activeScene == "MainMenu" || activeScene == "CharacterSelect")
            {
                return;
            }

            // Prevent saving if not logged in or if playing as Guest
            if (!FirebaseAuthManager.Instance.IsLoggedIn || FirebaseAuthManager.Instance.IsGuest)
            {
                return;
            }

            // Cooldown or active save check -> queue pending save so progress isn't lost
            if (_isSavingNow || Time.unscaledTime < _lastSaveTime + minSaveCooldown)
            {
                _pendingSaveReason = reason;
                if (!_hasPendingSave)
                {
                    _hasPendingSave = true;
                    StartCoroutine(ProcessPendingSaveCoroutine());
                }
                return;
            }

            ExecuteSave(reason);
        }

        private IEnumerator ProcessPendingSaveCoroutine()
        {
            while (_isSavingNow || Time.unscaledTime < _lastSaveTime + minSaveCooldown)
            {
                yield return new WaitForSecondsRealtime(0.5f);
            }
            if (_hasPendingSave)
            {
                _hasPendingSave = false;
                ExecuteSave(_pendingSaveReason);
            }
        }

        private void ExecuteSave(string reason)
        {
            _lastSaveTime = Time.unscaledTime;
            _isSavingNow = true;

            Debug.Log($"[AutoSaveManager] Saving game... (Reason: {reason})");

            // Show UI indicator
            AutoSaveUI.Instance.ShowSaving();

            // Perform Save
            FirebaseDatabaseManager.Instance.SyncFromGameAndSave((success, msg) =>
            {
                _isSavingNow = false;
                AutoSaveUI.Instance.OnSaveComplete(success);
                if (success)
                {
                    Debug.Log($"[AutoSaveManager] AutoSave complete! (Reason: {reason})");
                }
                else
                {
                    Debug.LogWarning($"[AutoSaveManager] AutoSave failed: {msg}");
                }
            });
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Trigger save when game pauses or user switches tabs/windows
                TriggerAutoSave("Application Paused / Minimized");
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // Trigger save when user clicks outside game window
                TriggerAutoSave("Application Lost Focus");
            }
        }

        private void OnApplicationQuit()
        {
            // Immediate local and cloud save right before exit for registered Google users
            if (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsGoogleUser)
            {
                Debug.Log("[AutoSaveManager] Saving right before Application Quit...");
                FirebaseDatabaseManager.Instance.SyncFromGameAndSave(null);
            }
        }
    }
}

