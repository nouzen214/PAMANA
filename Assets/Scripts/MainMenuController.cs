using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Management")]
        [SerializeField] private string gameSceneName = "CharacterSelect";

        [Header("Main Menu Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button logoutButton;
        [SerializeField] private TextMeshProUGUI accountStatusText;

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject loginPanel;

        [Header("Login UI Elements")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TextMeshProUGUI loginStatusText;

        [Header("Audio Settings Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;

        [Header("Google Connect in Settings")]
        [SerializeField] private Button connectGoogleButton;
        [SerializeField] private TextMeshProUGUI connectGoogleText;

        private void OnEnable()
        {
            UpdateMenuAccountState();
        }

        private void Start()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (loginPanel != null)
            {
                loginPanel.SetActive(false);
            }

            // Clean local cache state when on main menu
            if (FirebaseDatabaseManager.Instance != null)
            {
                FirebaseDatabaseManager.Instance.ClearAllLocalGameData();
            }

            InitializeSliders();
            EnsureVersionDisplay();
            UpdateMenuAccountState();
        }

        public void UpdateMenuAccountState()
        {
            bool isGoogle = (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsGoogleUser);
            if (logoutButton != null)
            {
                logoutButton.gameObject.SetActive(isGoogle);
            }
            if (accountStatusText != null)
            {
                if (isGoogle)
                {
                    string name = !string.IsNullOrEmpty(FirebaseAuthManager.Instance.Username) 
                        ? FirebaseAuthManager.Instance.Username 
                        : FirebaseAuthManager.Instance.Email;
                    accountStatusText.text = $"Logged in as: {name}";
                    accountStatusText.gameObject.SetActive(true);
                }
                else
                {
                    accountStatusText.gameObject.SetActive(false);
                }
            }
        }

        public void OnContinueClicked()
        {
            Debug.Log("[MainMenuController] Continue clicked");
            if (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsGoogleUser)
            {
                SetStatus("Loading cloud save...", isError: false);
                FirebaseDatabaseManager.Instance.LoadPlayerData((success, data) =>
                {
                    if (success && data != null)
                    {
                        ProceedToGame(data);
                    }
                    else
                    {
                        ProceedToGame();
                    }
                });
            }
            else
            {
                SetStatus("Sign in with Google to continue your saved game...", isError: false);
                FirebaseAuthManager.Instance.GoogleSignIn((success, msg) =>
                {
                    if (success)
                    {
                        UpdateMenuAccountState();
                        SetStatus("Google connected! Loading cloud save...", isError: false);
                        FirebaseDatabaseManager.Instance.LoadPlayerData((loadSuccess, data) =>
                        {
                            ProceedToGame(data);
                        });
                    }
                    else
                    {
                        SetStatus(msg ?? "Google sign-in cancelled or failed.", isError: true);
                    }
                });
            }
        }

        public void OnNewGameClicked()
        {
            Debug.Log("[MainMenuController] New Game clicked - opening login selection panel");
            if (loginPanel != null)
            {
                if (settingsPanel != null) settingsPanel.SetActive(false);
                loginPanel.SetActive(true);
            }
            else
            {
                ProceedToGame();
            }
        }

        public void OnLogoutClicked()
        {
            Debug.Log("[MainMenuController] Logout clicked");
            if (FirebaseAuthManager.Instance != null)
            {
                FirebaseAuthManager.Instance.SignOut();
            }
            if (FirebaseDatabaseManager.Instance != null)
            {
                FirebaseDatabaseManager.Instance.ClearAllLocalGameData();
            }
            UpdateMenuAccountState();
            SetStatus("Logged out of Google account.", isError: false);
        }

        private void EnsureVersionDisplay()
        {
            var canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var existing = canvas.transform.Find("VersionLabel");
            if (existing == null)
            {
                var go = new GameObject("VersionLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(canvas.transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(1f, 0f);
                rt.anchoredPosition = new Vector2(-16f, 12f);
                rt.sizeDelta = new Vector2(160f, 30f);

                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = $"v{Application.version}";
                tmp.fontSize = 16f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.BottomRight;
                tmp.color = new Color(0.95f, 0.95f, 0.95f, 0.85f);
                tmp.raycastTarget = false;
            }
        }

        private void InitializeSliders()
        {
            AudioSettingsManager.Load();

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = AudioSettingsManager.MasterVolume;
                masterVolumeSlider.onValueChanged.RemoveAllListeners();
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioSettingsManager.SFXVolume;
                sfxVolumeSlider.onValueChanged.RemoveAllListeners();
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = AudioSettingsManager.BGMVolume;
                bgmVolumeSlider.onValueChanged.RemoveAllListeners();
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }
        }

        public void OnStartGameClicked()
        {
            OnNewGameClicked();
        }

        public void OnCloseLoginClicked()
        {
            if (loginPanel != null)
            {
                loginPanel.SetActive(false);
            }
        }

        public void OnLoginClicked()
        {
            string user = usernameInput != null ? usernameInput.text : "";
            string pass = passwordInput != null ? passwordInput.text : "";

            SetStatus("Logging in...", isError: false);
            FirebaseAuthManager.Instance.Login(user, pass, (success, msg) =>
            {
                if (success)
                {
                    SetStatus("Login successful! Loading data...", isError: false);
                    FirebaseDatabaseManager.Instance.ClearAllLocalGameData();
                    FirebaseDatabaseManager.Instance.LoadPlayerData((loadSuccess, data) =>
                    {
                        ProceedToGame(data);
                    });
                }
                else
                {
                    SetStatus(msg, isError: true);
                }
            });
        }

        public void OnRegisterClicked()
        {
            string user = usernameInput != null ? usernameInput.text : "";
            string pass = passwordInput != null ? passwordInput.text : "";

            SetStatus("Creating account...", isError: false);
            FirebaseAuthManager.Instance.Register(user, pass, (success, msg) =>
            {
                if (success)
                {
                    SetStatus("Account created! Initializing fresh account...", isError: false);
                    // Brand-new registration: wipe all local data and initialize completely fresh account data
                    FirebaseDatabaseManager.Instance.ClearAllLocalGameData();
                    var freshData = FirebaseDatabaseManager.Instance.CreateFreshPlayerData();
                    freshData.hasSelectedCharacter = false;
                    freshData.selectedCharacter = "";
                    FirebaseDatabaseManager.Instance.SavePlayerData(freshData, (saveSuccess, saveMsg) =>
                    {
                        ProceedToGame(freshData);
                    });
                }
                else
                {
                    SetStatus(msg, isError: true);
                }
            });
        }

        public void OnGoogleLoginClicked()
        {
            SetStatus("Connecting with Google...", isError: false);
            FirebaseAuthManager.Instance.GoogleSignIn((success, msg) =>
            {
                if (success)
                {
                    SetStatus("Google login successful! Loading...", isError: false);
                    FirebaseDatabaseManager.Instance.ClearAllLocalGameData();
                    FirebaseDatabaseManager.Instance.LoadPlayerData((loadSuccess, data) =>
                    {
                        ProceedToGame(data);
                    });
                }
                else
                {
                    SetStatus(msg, isError: true);
                }
            });
        }

        public void OnGuestLoginClicked()
        {
            SetStatus("Entering as Guest...", isError: false);
            FirebaseAuthManager.Instance.GuestSignIn((success, msg) =>
            {
                if (success)
                {
                    SetStatus("Welcome, Guest! Loading game...", isError: false);
                    FirebaseDatabaseManager.Instance.ClearAllLocalGameData();
                    var guestData = FirebaseDatabaseManager.Instance.CreateFreshPlayerData();
                    FirebaseDatabaseManager.Instance.ApplyDataToGame(guestData);
                    ProceedToGame(guestData);
                }
                else
                {
                    SetStatus(msg, isError: true);
                }
            });
        }

        public void OnForgotPasswordClicked()
        {
            string user = usernameInput != null ? usernameInput.text : "";
            if (string.IsNullOrWhiteSpace(user))
            {
                SetStatus("Enter your username or email above first.", isError: true);
                return;
            }

            SetStatus("Sending reset email...", isError: false);
            FirebaseAuthManager.Instance.ResetPassword(user, (success, msg) =>
            {
                SetStatus(msg, isError: !success);
            });
        }

        private void SetStatus(string message, bool isError = false)
        {
            if (loginStatusText != null)
            {
                loginStatusText.text = message;
                loginStatusText.color = isError 
                    ? new Color(0.85f, 0.15f, 0.15f, 1f) 
                    : new Color(0.15f, 0.55f, 0.15f, 1f);
                loginStatusText.gameObject.SetActive(true);
            }
            Debug.Log($"[Login Status] {message}");
        }

        public void ProceedToGame(PlayerData data = null)
        {
            Debug.Log("Proceeding to Game...");
            Time.timeScale = 1f;

            if (data == null)
            {
                data = FirebaseDatabaseManager.Instance.CurrentPlayerData;
            }

            // ONLY consider already selected if THIS SPECIFIC ACCOUNT has selected a character!
            bool alreadySelected = (data != null && data.hasSelectedCharacter && !string.IsNullOrEmpty(data.selectedCharacter));

            if (alreadySelected)
            {
                // Returning user: Do not make them pick a character again!
                string chosenChar = data.selectedCharacter;
                PlayerPrefs.SetString("SelectedCharacter", chosenChar);
                PlayerPrefs.SetInt("HasSelectedCharacter", 1);
                PlayerPrefs.Save();

                string destinationScene = "Lobby";
                if (!string.IsNullOrEmpty(data.currentScene) &&
                    data.currentScene != "MainMenu" && data.currentScene != "CharacterSelect")
                {
                    destinationScene = data.currentScene;
                }

                Debug.Log($"[MainMenuController] Returning user ({chosenChar}). Skipping CharacterSelect -> Loading scene: {destinationScene}");
                SceneManager.LoadScene(destinationScene);
            }
            else
            {
                // First-time player on this account: Must pick character!
                Debug.Log("[MainMenuController] First-time player on this account. Directing to CharacterSelect...");
                FirebaseDatabaseManager.Instance.ClearAllLocalGameData();
                PlayerPrefs.SetInt("HasSelectedCharacter", 0);
                PlayerPrefs.Save();
                SceneManager.LoadScene(gameSceneName); // "CharacterSelect"
            }
        }

        public void OnSettingsClicked()
        {
            if (settingsPanel != null)
            {
                InitializeSliders();
                UpdateGoogleConnectUI();
                settingsPanel.SetActive(true);
            }
        }

        public void OnConnectToGoogleClicked()
        {
            if (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsGoogleUser)
            {
                string prevUser = FirebaseAuthManager.Instance.Username;
                FirebaseAuthManager.Instance.UnbindGoogle();
                UpdateGoogleConnectUI();
                SetStatus($"Google account ({prevUser}) unbound. You can now connect a different account.", isError: false);
                return;
            }

            if (connectGoogleText != null)
            {
                connectGoogleText.text = "CONNECTING...";
            }

            OnGoogleLoginClicked();
        }

        public void UpdateGoogleConnectUI()
        {
            if (connectGoogleText != null)
            {
                if (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsGoogleUser)
                {
                    connectGoogleText.text = $"UNBIND: {FirebaseAuthManager.Instance.Username.ToUpper()}";
                }
                else
                {
                    connectGoogleText.text = "";
                }
            }
        }

        public void OnCloseSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void OnSaveSettingsClicked()
        {
            AudioSettingsManager.Save();
            OnCloseSettingsClicked();
        }

        public void OnDefaultSettingsClicked()
        {
            AudioSettingsManager.ResetDefaults();
            if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
            if (bgmVolumeSlider != null) bgmVolumeSlider.value = 1f;
        }

        public void OnMasterVolumeChanged(float volume)
        {
            AudioSettingsManager.SetMaster(volume);
        }

        public void OnSFXVolumeChanged(float volume)
        {
            AudioSettingsManager.SetSFX(volume);
        }

        public void OnBGMVolumeChanged(float volume)
        {
            AudioSettingsManager.SetBGM(volume);
        }

        public void SetVolume(float volume)
        {
            OnMasterVolumeChanged(volume);
        }

        public void OnExitClicked()
        {
            Debug.Log("Exit Game clicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            Application.ExternalEval("window.location.reload();");
#else
            Application.Quit();
#endif
        }
    }
}
