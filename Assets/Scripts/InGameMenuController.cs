using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    public class InGameMenuController : MonoBehaviour
    {
        public static InGameMenuController Instance { get; private set; }
        public static bool IsSettingsOpen => Instance != null && Instance.settingsPanel != null && Instance.settingsPanel.activeSelf;

        [Header("Scene Config")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private BagModalUI bagModal;

        [Header("Audio Settings Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;

        [Header("Google Connect")]
        [SerializeField] private Button connectGoogleButton;
        [SerializeField] private TextMeshProUGUI connectGoogleText;

        [Header("HUD Buttons")]
        [SerializeField] private Button bookButton;
        public Button BookButton => bookButton;

        private void Awake()
        {
            Instance = this;
            FindBookButton();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            GameProgression.OnBookUnlocked += UpdateBookButtonState;
            UpdateBookButtonState();
        }

        private void OnDisable()
        {
            GameProgression.OnBookUnlocked -= UpdateBookButtonState;
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                Time.timeScale = 1f;
                SetPlayerMovementState(true);
            }
        }

        public void FindBookButton()
        {
            if (bookButton == null)
            {
                var t = transform.Find("BookButton") ?? transform.Find("Canvas/BookButton");
                if (t == null)
                {
                    var canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
                    if (canvas != null) t = canvas.transform.Find("BookButton");
                }
                if (t != null) bookButton = t.GetComponent<Button>();
            }
        }

        public void UpdateBookButtonState()
        {
            FindBookButton();
            if (bookButton != null)
            {
                bookButton.gameObject.SetActive(GameProgression.IsBookUnlocked);
            }
        }

        private void Start()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (bagModal == null)
            {
                bagModal = FindFirstObjectByType<BagModalUI>(FindObjectsInactive.Include);
            }

            InitializeSliders();
        }

        private void InitializeSliders()
        {
            AudioSettingsManager.Load();

            SetupSlider(masterVolumeSlider, AudioSettingsManager.MasterVolume, OnMasterVolumeChanged);
            SetupSlider(sfxVolumeSlider, AudioSettingsManager.SFXVolume, OnSFXVolumeChanged);
            SetupSlider(bgmVolumeSlider, AudioSettingsManager.BGMVolume, OnBGMVolumeChanged);
        }

        private void SetupSlider(Slider slider, float initialValue, UnityEngine.Events.UnityAction<float> onChanged)
        {
            if (slider == null) return;

            var nav = slider.navigation;
            nav.mode = Navigation.Mode.None;
            slider.navigation = nav;

            if (slider.GetComponent<SliderNoKeyboardNavigation>() == null)
            {
                slider.gameObject.AddComponent<SliderNoKeyboardNavigation>();
            }

            slider.value = initialValue;
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(onChanged);
        }

        private void SetPlayerMovementState(bool canMove)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var movement = player.GetComponent<PlayerMovement>();
                if (movement != null) movement.enabled = canMove;

                var input = player.GetComponent<PlayerInputHandler>();
                if (input != null) input.enabled = canMove;

                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;

                var anim = player.GetComponent<CharacterAnimator2D>();
                if (anim != null && !canMove) anim.SetIdlePose();
            }
        }

        private void Update()
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                // Ensure no slider remains selected in EventSystem while settings is open
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
                {
                    var selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                    if (selected.GetComponent<Slider>() != null)
                    {
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                    }
                }

                // Allow Escape to close settings
                bool escapePressed = false;
                if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    escapePressed = true;
                }
                else
                {
                    try { if (Input.GetKeyDown(KeyCode.Escape)) escapePressed = true; } catch { }
                }

                if (escapePressed)
                {
                    OnCloseSettingsClicked();
                }
            }
        }

        public void OnSettingsClicked()
        {
            if (bagModal != null && bagModal.IsOpen)
            {
                bagModal.CloseBag();
            }

            if (BookModalUI.Instance != null && BookModalUI.Instance.IsOpen)
            {
                BookModalUI.Instance.CloseBook();
            }

            if (settingsPanel != null)
            {
                bool willOpen = !settingsPanel.activeSelf;
                if (willOpen)
                {
                    InitializeSliders();
                    UpdateGoogleConnectUI();
                }
                settingsPanel.SetActive(willOpen);
                Time.timeScale = willOpen ? 0f : 1f;
                SetPlayerMovementState(!willOpen);

                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                }
            }
            else
            {
                GoToMainMenu();
            }
        }

        public void OnConnectToGoogleClicked()
        {
            if (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsGoogleUser)
            {
                // Account is currently connected to Google -> UNBIND
                string prevUser = FirebaseAuthManager.Instance.Username;

                // Sync and save progress to the current Google account before unbinding
                if (FirebaseDatabaseManager.Instance != null)
                {
                    FirebaseDatabaseManager.Instance.SyncFromGameAndSave(null);
                }

                FirebaseAuthManager.Instance.UnbindGoogle();
                UpdateGoogleConnectUI();
                NotificationBannerUI.Show("Google Unbound", $"Disconnected from {prevUser}. You can now connect a different Google account or play as guest.");
                return;
            }

            // Not connected to Google (Playing as Guest) -> BIND TO GOOGLE
            if (connectGoogleText != null)
            {
                connectGoogleText.text = "CONNECTING...";
            }

            FirebaseAuthManager.Instance.GoogleSignIn((success, msg) =>
            {
                if (success)
                {
                    // Successfully authenticated with Google!
                    // Bind the guest's active game progress directly to this Google account in Firebase!
                    if (FirebaseDatabaseManager.Instance != null)
                    {
                        FirebaseDatabaseManager.Instance.SyncFromGameAndSave((saveSuccess, saveMsg) =>
                        {
                            UpdateGoogleConnectUI();
                            NotificationBannerUI.Show("Account Bound!", $"Connected as {FirebaseAuthManager.Instance.Username}! Progress saved to cloud.");
                        });
                    }
                    else
                    {
                        UpdateGoogleConnectUI();
                        NotificationBannerUI.Show("Account Bound!", $"Connected as {FirebaseAuthManager.Instance.Username}!");
                    }
                }
                else
                {
                    UpdateGoogleConnectUI();
                    NotificationBannerUI.Show("Google Sign-In", msg);
                }
            });
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
            Time.timeScale = 1f;
            SetPlayerMovementState(true);

            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
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

        public void OnBagClicked()
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
                Time.timeScale = 1f;
                SetPlayerMovementState(true);
            }

            if (BookModalUI.Instance != null && BookModalUI.Instance.IsOpen)
            {
                BookModalUI.Instance.CloseBook();
            }

            if (bagModal == null)
            {
                bagModal = FindFirstObjectByType<BagModalUI>(FindObjectsInactive.Include);
            }

            if (bagModal == null)
            {
                bagModal = BagModalUI.CreateBagModal(transform);
            }

            if (bagModal != null)
            {
                if (bagModal.IsOpen)
                {
                    bagModal.CloseBag();
                }
                else
                {
                    bagModal.OpenBag();
                }
            }
        }

        public void OnBookClicked()
        {
            if (!GameProgression.IsBookUnlocked)
            {
                NotificationBannerUI.Show(
                    "FIELD GUIDE LOCKED",
                    "Inspect the book pedestal in the Main Lobby to unlock the Field Guide!",
                    3.5f,
                    new Color(0.95f, 0.35f, 0.35f, 1f),
                    "!"
                );
                return;
            }

            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
                SetPlayerMovementState(true);
            }

            if (bagModal != null && bagModal.IsOpen)
            {
                bagModal.CloseBag();
            }

            if (BookModalUI.Instance != null && BookModalUI.Instance.IsOpen)
            {
                BookModalUI.Instance.CloseBook();
            }
            else
            {
                BookModalUI.Show();
            }
        }

        public void OnTutorialClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
                SetPlayerMovementState(true);
            }
            if (bagModal != null && bagModal.IsOpen)
            {
                bagModal.CloseBag();
            }
            BookModalUI.Show();
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;

            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene != "MainMenu" && currentScene != "CharacterSelect")
            {
                PlayerPrefs.SetString("LastActiveScene", currentScene);
                PlayerPrefs.Save();
            }

            if (FirebaseDatabaseManager.Instance != null)
            {
                FirebaseDatabaseManager.Instance.SyncFromGameAndSave(null);
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void RestartCurrentScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
