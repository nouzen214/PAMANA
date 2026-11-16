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
            string prevGuestId = FirebaseAuthManager.Instance != null ? FirebaseAuthManager.Instance.UserId : "";
            string prevGuestName = FirebaseAuthManager.Instance != null ? FirebaseAuthManager.Instance.Username : "Guest";

            if (connectGoogleText != null)
            {
                connectGoogleText.text = "CONNECTING...";
            }

            FirebaseAuthManager.Instance.GoogleSignIn((success, msg) =>
            {
                if (!success)
                {
                    UpdateGoogleConnectUI();
                    NotificationBannerUI.Show("Google Sign-In", msg);
                    return;
                }

                // Successfully authenticated with Google!
                string googleUserId = FirebaseAuthManager.Instance.UserId;
                string googleToken = FirebaseAuthManager.Instance.IdToken;
                string googleUsername = FirebaseAuthManager.Instance.Username;

                // Check whether this Google account ALREADY HAS an existing cloud save
                if (FirebaseDatabaseManager.Instance != null)
                {
                    FirebaseDatabaseManager.Instance.CheckCloudSaveExists(googleUserId, googleToken, (checkSuccess, hasProgress, cloudData) =>
                    {
                        if (!checkSuccess)
                        {
                            // Could not verify cloud data; defensively revert to guest to avoid overwriting
                            FirebaseAuthManager.Instance.RestoreGuestSession(prevGuestId, prevGuestName);
                            UpdateGoogleConnectUI();
                            NotificationBannerUI.Show("Connection Error", "Could not check cloud save data. Preserved guest session.");
                            return;
                        }

                        if (!hasProgress)
                        {
                            // Fresh Google account with no previous game progress -> Safe to bind guest progress!
                            FirebaseDatabaseManager.Instance.SyncFromGameAndSave((saveSuccess, saveMsg) =>
                            {
                                UpdateGoogleConnectUI();
                                NotificationBannerUI.Show("Account Bound!", $"Connected as {googleUsername}! Progress saved to cloud.");
                            });
                        }
                        else
                        {
                            // Account is ALREADY REGISTERED with existing game progress!
                            // DO NOT overwrite! Present conflict modal allowing user to Load Cloud Save or Cancel.
                            ShowAccountConflictModal(cloudData, googleUsername, prevGuestId, prevGuestName);
                        }
                    });
                }
                else
                {
                    UpdateGoogleConnectUI();
                    NotificationBannerUI.Show("Account Bound!", $"Connected as {googleUsername}!");
                }
            });
        }

        private GameObject _accountConflictModal;

        private void ShowAccountConflictModal(PlayerData cloudData, string googleUsername, string prevGuestId, string prevGuestName)
        {
            if (_accountConflictModal != null)
            {
                Destroy(_accountConflictModal);
            }

            Canvas targetCanvas = GetComponentInParent<Canvas>();
            if (targetCanvas == null)
            {
                targetCanvas = FindFirstObjectByType<Canvas>();
            }

            if (targetCanvas == null)
            {
                // Fallback if no canvas: revert to guest and notify
                FirebaseAuthManager.Instance.RestoreGuestSession(prevGuestId, prevGuestName);
                UpdateGoogleConnectUI();
                NotificationBannerUI.Show("Account Already Registered", $"This Google account already has saved progress. Preserved cloud save.");
                return;
            }

            // Dark semi-transparent blocker overlay
            var overlayGo = new GameObject("AccountConflictModal", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            overlayGo.transform.SetParent(targetCanvas.transform, false);
            _accountConflictModal = overlayGo;

            var overlayRt = overlayGo.GetComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.sizeDelta = Vector2.zero;
            overlayRt.anchoredPosition = Vector2.zero;

            var overlayImg = overlayGo.GetComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.78f);
            overlayImg.raycastTarget = true;

            // Frame container (ornate frame)
            var frameGo = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            frameGo.transform.SetParent(overlayGo.transform, false);

            var frameRt = frameGo.GetComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0.5f, 0.5f);
            frameRt.anchorMax = new Vector2(0.5f, 0.5f);
            frameRt.pivot = new Vector2(0.5f, 0.5f);
            frameRt.sizeDelta = new Vector2(520f, 390f);
            frameRt.anchoredPosition = Vector2.zero;

            var frameImg = frameGo.GetComponent<Image>();
            Sprite confirmSprite = Resources.Load<Sprite>("UI/Pause_Confirm_Frame");
            if (confirmSprite != null)
            {
                frameImg.sprite = confirmSprite;
                frameImg.type = Image.Type.Sliced;
                frameImg.color = Color.white;
            }
            else
            {
                frameImg.color = new Color(0.15f, 0.10f, 0.05f, 0.96f);
            }

            var frameOutline = frameGo.AddComponent<Outline>();
            frameOutline.effectColor = new Color(0.85f, 0.65f, 0.25f, 0.85f);
            frameOutline.effectDistance = new Vector2(2f, -2f);

            // Title
            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(frameGo.transform, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -28f);
            titleRt.sizeDelta = new Vector2(-40f, 36f);

            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "ACCOUNT ALREADY REGISTERED";
            titleTmp.fontSize = 20f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = new Color(1f, 0.82f, 0.35f, 1f);
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Description / Details
            string charDesc = !string.IsNullOrEmpty(cloudData.characterName)
                ? cloudData.characterName
                : (!string.IsNullOrEmpty(cloudData.selectedCharacter) ? cloudData.selectedCharacter : "Hero");

            int roomsCleared = 0;
            if (cloudData.roomProgress != null)
            {
                foreach (var rp in cloudData.roomProgress)
                {
                    if (rp != null && (rp.battleCompleted || rp.artifactRetrieved)) roomsCleared++;
                }
            }

            string sceneName = !string.IsNullOrEmpty(cloudData.currentScene) ? cloudData.currentScene : "Lobby";

            var descGo = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
            descGo.transform.SetParent(frameGo.transform, false);
            var descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 0f);
            descRt.anchorMax = new Vector2(1f, 1f);
            descRt.offsetMin = new Vector2(32f, 90f);
            descRt.offsetMax = new Vector2(-32f, -70f);

            var descTmp = descGo.GetComponent<TextMeshProUGUI>();
            descTmp.text = $"This Google account (<b>{googleUsername}</b>) is already registered with existing cloud progress:\n\n" +
                           $"• <b>Character:</b> {charDesc}\n" +
                           $"• <b>Saved Scene:</b> {sceneName}\n" +
                           $"• <b>Progress:</b> {roomsCleared} Room(s) Explored\n\n" +
                           $"Would you like to <b>Load Cloud Save</b> to restore your previous game, or <b>Cancel</b> to stay as Guest without overwriting anything?";
            descTmp.fontSize = 14f;
            descTmp.lineSpacing = 6f;
            descTmp.color = new Color(0.95f, 0.90f, 0.82f, 1f);
            descTmp.alignment = TextAlignmentOptions.Center;

            // Button 1: Load Cloud Save
            var loadBtnGo = CreateModalButton(frameGo.transform, "LoadSaveButton", "LOAD CLOUD SAVE", new Vector2(-115f, -145f), new Vector2(200f, 44f),
                new Color(0.12f, 0.32f, 0.16f, 0.95f), new Color(0.35f, 0.85f, 0.45f, 1f));
            var loadBtn = loadBtnGo.GetComponent<Button>();
            loadBtn.onClick.AddListener(() =>
            {
                if (_accountConflictModal != null) Destroy(_accountConflictModal);
                if (settingsPanel != null) settingsPanel.SetActive(false);

                // Apply the saved cloud data to the game
                FirebaseDatabaseManager.Instance.ApplyDataToGame(cloudData, clearExisting: true);
                UpdateGoogleConnectUI();
                NotificationBannerUI.Show("Cloud Save Loaded!", $"Welcome back, {googleUsername}! Restored your saved game.");

                // Transition to the player's saved scene
                string targetScene = !string.IsNullOrEmpty(cloudData.currentScene) ? cloudData.currentScene : "Lobby";
                Time.timeScale = 1f;
                SceneManager.LoadScene(targetScene);
            });

            // Button 2: Cancel (Stay as Guest)
            var cancelBtnGo = CreateModalButton(frameGo.transform, "CancelButton", "CANCEL", new Vector2(115f, -145f), new Vector2(180f, 44f),
                new Color(0.35f, 0.12f, 0.12f, 0.95f), new Color(0.90f, 0.35f, 0.35f, 1f));
            var cancelBtn = cancelBtnGo.GetComponent<Button>();
            cancelBtn.onClick.AddListener(() =>
            {
                if (_accountConflictModal != null) Destroy(_accountConflictModal);

                // Revert back to the guest session so cloud save is preserved untouched
                FirebaseAuthManager.Instance.RestoreGuestSession(prevGuestId, prevGuestName);
                UpdateGoogleConnectUI();
                NotificationBannerUI.Show("Account Already Registered", "Cloud save preserved. Kept playing as Guest.");
            });
        }

        private GameObject CreateModalButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Color bgColor, Color outlineColor)
        {
            var btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);

            var rt = btnGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = btnGo.GetComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            var outline = btnGo.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var btn = btnGo.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            btn.colors = colors;

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            var tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 13.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return btnGo;
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
            if (_accountConflictModal != null)
            {
                Destroy(_accountConflictModal);
                if (FirebaseAuthManager.Instance != null && !FirebaseAuthManager.Instance.IsGuest)
                {
                    FirebaseAuthManager.Instance.RestoreGuestSession();
                    UpdateGoogleConnectUI();
                }
            }

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
