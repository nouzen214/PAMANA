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

        private static bool IsAnyInputFieldFocused()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                var selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                if (selected != null)
                {
                    if (selected.GetComponent<TMPro.TMP_InputField>() != null || selected.GetComponent<UnityEngine.UI.InputField>() != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Update()
        {
            if (ArtifactInspectionUI.Instance != null && ArtifactInspectionUI.Instance.IsOpen)
            {
                return;
            }

            if (IsAnyInputFieldFocused())
            {
                return;
            }

            // 1. Keyboard Shortcut: H key -> Open / Toggle History Book
            bool hPressed = false;
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.hKey.wasPressedThisFrame)
            {
                hPressed = true;
            }
            if (!hPressed)
            {
                try { if (Input.GetKeyDown(KeyCode.H)) hPressed = true; } catch { }
            }

            if (hPressed)
            {
                OnBookClicked();
                return;
            }

            // 2. Keyboard Shortcut: B key -> Open / Toggle Bag (also supports I key for inventory)
            bool bPressed = false;
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                (UnityEngine.InputSystem.Keyboard.current.bKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame))
            {
                bPressed = true;
            }
            if (!bPressed)
            {
                try { if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.I)) bPressed = true; } catch { }
            }

            if (bPressed)
            {
                OnBagClicked();
                return;
            }

            // 3. Escape key -> Close open modal (Settings, Book, or Bag)
            bool escapePressed = false;
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                escapePressed = true;
            }
            if (!escapePressed)
            {
                try { if (Input.GetKeyDown(KeyCode.Escape)) escapePressed = true; } catch { }
            }

            if (escapePressed)
            {
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    OnCloseSettingsClicked();
                    return;
                }
                if (BookModalUI.Instance != null && BookModalUI.Instance.IsOpen)
                {
                    BookModalUI.Instance.CloseBook();
                    return;
                }
                if (bagModal == null)
                {
                    bagModal = FindFirstObjectByType<BagModalUI>(FindObjectsInactive.Include);
                }
                if (bagModal != null && bagModal.IsOpen)
                {
                    bagModal.CloseBag();
                    return;
                }
            }

            // Keep settings slider selection clean while settings panel is open
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

                if (connectGoogleButton != null)
                {
                    connectGoogleButton.interactable = false;
                }
                if (connectGoogleText != null)
                {
                    connectGoogleText.text = "SAVING...";
                }

                // Sync and save progress to the current Google account BEFORE unbinding
                if (FirebaseDatabaseManager.Instance != null)
                {
                    FirebaseDatabaseManager.Instance.SyncFromGameAndSave((saveSuccess, saveMsg) =>
                    {
                        if (connectGoogleButton != null)
                        {
                            connectGoogleButton.interactable = true;
                        }

                        FirebaseAuthManager.Instance.UnbindGoogle();
                        UpdateGoogleConnectUI();
                        if (saveSuccess)
                        {
                            NotificationBannerUI.Show("Google Unbound", $"Progress saved to {prevUser}! You can now play as guest or connect a different account.");
                        }
                        else
                        {
                            NotificationBannerUI.Show("Google Unbound", $"Disconnected from {prevUser}. You can now connect a different Google account or play as guest.");
                        }
                    });
                }
                else
                {
                    if (connectGoogleButton != null) connectGoogleButton.interactable = true;
                    FirebaseAuthManager.Instance.UnbindGoogle();
                    UpdateGoogleConnectUI();
                    NotificationBannerUI.Show("Google Unbound", $"Disconnected from {prevUser}. You can now connect a different Google account or play as guest.");
                }
                return;
            }

            // Not connected to Google (Playing as Guest) -> BIND TO GOOGLE
            string prevGuestId = FirebaseAuthManager.Instance != null ? FirebaseAuthManager.Instance.UserId : "";
            string prevGuestName = FirebaseAuthManager.Instance != null ? FirebaseAuthManager.Instance.Username : "Guest";

            // Pause AutoSave during Google Sign-In and conflict modal verification
            AutoSaveManager.IsAutoSavePaused = true;

            if (connectGoogleText != null)
            {
                connectGoogleText.text = "CONNECTING...";
            }

            FirebaseAuthManager.Instance.GoogleSignIn((success, msg) =>
            {
                if (!success)
                {
                    AutoSaveManager.IsAutoSavePaused = false;
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
                            AutoSaveManager.IsAutoSavePaused = false;
                            FirebaseAuthManager.Instance.RestoreGuestSession(prevGuestId, prevGuestName);
                            UpdateGoogleConnectUI();
                            NotificationBannerUI.Show("Connection Error", "Could not check cloud save data. Preserved guest session.");
                            return;
                        }

                        if (!hasProgress)
                        {
                            // Fresh Google account with no previous game progress -> Safe to bind guest progress!
                            PlayerPrefs.SetInt("HasGuestSave", 0);
                            PlayerPrefs.DeleteKey("GuestUserId");
                            PlayerPrefs.Save();

                            FirebaseDatabaseManager.Instance.SyncFromGameAndSave((saveSuccess, saveMsg) =>
                            {
                                AutoSaveManager.IsAutoSavePaused = false;
                                UpdateGoogleConnectUI();
                                NotificationBannerUI.Show("Account Bound!", $"Connected as {googleUsername}! Progress saved to cloud.");
                            });
                        }
                        else
                        {
                            // Account is ALREADY REGISTERED with existing game progress!
                            // Keep AutoSave paused so background events cannot overwrite cloud save.
                            // DO NOT overwrite! Present conflict modal allowing user to Load Cloud Save or Cancel.
                            ShowAccountConflictModal(cloudData, googleUserId, googleUsername, googleToken, prevGuestId, prevGuestName);
                        }
                    });
                }
                else
                {
                    AutoSaveManager.IsAutoSavePaused = false;
                    UpdateGoogleConnectUI();
                    NotificationBannerUI.Show("Account Bound!", $"Connected as {googleUsername}!");
                }
            });
        }

        private GameObject _accountConflictModal;

        private void ShowAccountConflictModal(PlayerData cloudData, string googleUserId, string googleUsername, string googleToken, string prevGuestId, string prevGuestName)
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

            // Hide the SettingsPanel so it doesn't clutter behind the conflict modal
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // Dark semi-transparent blocker overlay
            var overlayGo = new GameObject("AccountConflictModal", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            overlayGo.transform.SetParent(targetCanvas.transform, false);
            _accountConflictModal = overlayGo;

            var overlayRt = overlayGo.GetComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;

            var overlayImg = overlayGo.GetComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.85f);
            overlayImg.raycastTarget = true;

            // Frame container (Royal Parchment Scroll Frame: 780 x 562)
            var frameGo = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            frameGo.transform.SetParent(overlayGo.transform, false);

            var frameRt = frameGo.GetComponent<RectTransform>();
            frameRt.anchorMin = new Vector2(0.5f, 0.5f);
            frameRt.anchorMax = new Vector2(0.5f, 0.5f);
            frameRt.pivot = new Vector2(0.5f, 0.5f);
            frameRt.sizeDelta = new Vector2(780f, 562f);
            frameRt.anchoredPosition = Vector2.zero;

            var frameImg = frameGo.GetComponent<Image>();
            Sprite scrollSprite = Resources.Load<Sprite>("UI/AccountConflict_Scroll_Frame");
            if (scrollSprite == null)
            {
                scrollSprite = Resources.Load<Sprite>("UI/Pause_Scroll_Frame");
            }
            if (scrollSprite != null)
            {
                frameImg.sprite = scrollSprite;
                frameImg.type = Image.Type.Simple;
                frameImg.preserveAspect = true;
                frameImg.color = Color.white;
            }
            else
            {
                frameImg.color = new Color(0.15f, 0.10f, 0.05f, 0.96f);
            }

            // Title directly inside the carved wooden plaque
            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(frameGo.transform, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.anchoredPosition = new Vector2(0f, -122f);
            titleRt.sizeDelta = new Vector2(260f, 32f);

            var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "CLOUD SAVE FOUND";
            titleTmp.fontSize = 17.5f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = new Color(1f, 0.88f, 0.52f, 1f);
            titleTmp.alignment = TextAlignmentOptions.Center;
            var titleShadow = titleGo.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(0.12f, 0.08f, 0.05f, 0.95f);
            titleShadow.effectDistance = new Vector2(1.5f, -1.5f);

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

            // Recessed Inset Card for Cloud Details
            var cardGo = new GameObject("DetailsCard", typeof(RectTransform), typeof(Image), typeof(Outline));
            cardGo.transform.SetParent(frameGo.transform, false);

            var cardRt = cardGo.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(510f, 142f);
            cardRt.anchoredPosition = new Vector2(0f, 40f);

            var cardImg = cardGo.GetComponent<Image>();
            cardImg.color = new Color(0.90f, 0.81f, 0.67f, 0.75f);

            var cardOutline = cardGo.GetComponent<Outline>();
            cardOutline.effectColor = new Color(0.48f, 0.32f, 0.18f, 0.85f);
            cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Details Text inside Card
            var detailsGo = new GameObject("DetailsText", typeof(RectTransform), typeof(TextMeshProUGUI));
            detailsGo.transform.SetParent(cardGo.transform, false);

            var detailsRt = detailsGo.GetComponent<RectTransform>();
            detailsRt.anchorMin = Vector2.zero;
            detailsRt.anchorMax = Vector2.one;
            detailsRt.offsetMin = new Vector2(18f, 10f);
            detailsRt.offsetMax = new Vector2(-18f, -10f);

            var detailsTmp = detailsGo.GetComponent<TextMeshProUGUI>();
            detailsTmp.fontSize = 15.5f;
            detailsTmp.lineSpacing = 8f;
            detailsTmp.color = new Color(0.24f, 0.15f, 0.08f, 1f);
            detailsTmp.alignment = TextAlignmentOptions.Center;
            detailsTmp.text = "<b><color=#723d14>REGISTERED GOOGLE ACCOUNT</color></b>\n" +
                              $"<b>Account:</b> <color=#944a10><b>{googleUsername}</b></color>\n" +
                              $"<b>Character:</b> {charDesc}  •  <b>Saved Location:</b> {sceneName}\n" +
                              $"<b>Progress:</b> <color=#286828><b>{roomsCleared} Room(s) Explored</b></color>";

            // Prompt Text below Card
            var promptGo = new GameObject("PromptText", typeof(RectTransform), typeof(TextMeshProUGUI));
            promptGo.transform.SetParent(frameGo.transform, false);

            var promptRt = promptGo.GetComponent<RectTransform>();
            promptRt.anchorMin = new Vector2(0.5f, 0.5f);
            promptRt.anchorMax = new Vector2(0.5f, 0.5f);
            promptRt.pivot = new Vector2(0.5f, 0.5f);
            promptRt.sizeDelta = new Vector2(510f, 52f);
            promptRt.anchoredPosition = new Vector2(0f, -68f);

            var promptTmp = promptGo.GetComponent<TextMeshProUGUI>();
            promptTmp.fontSize = 14f;
            promptTmp.lineSpacing = 5f;
            promptTmp.color = new Color(0.28f, 0.18f, 0.10f, 0.95f);
            promptTmp.alignment = TextAlignmentOptions.Center;
            promptTmp.text = "Would you like to <b>Load Cloud Save</b> to restore your previous progress,\nor <b>Stay as Guest</b> to continue playing without overwriting anything?";

            Sprite sprBtnNormal = Resources.Load<Sprite>("UI/Btn_WoodPlank_Blank_Normal");
            Sprite sprBtnHover = Resources.Load<Sprite>("UI/Btn_WoodPlank_Blank_Hover");

            // Button 1: Load Cloud Save
            var loadBtnGo = CreateWoodenModalButton(frameGo.transform, "Button_LoadCloud", "LOAD CLOUD SAVE",
                new Vector2(-130f, -148f), new Vector2(235f, 62f), sprBtnNormal, sprBtnHover, new Color(1f, 0.88f, 0.45f, 1f));
            var loadBtn = loadBtnGo.GetComponent<Button>();
            loadBtn.onClick.AddListener(() =>
            {
                if (_accountConflictModal != null) Destroy(_accountConflictModal);
                if (settingsPanel != null) settingsPanel.SetActive(false);

                // Explicitly activate Google session credentials
                if (FirebaseAuthManager.Instance != null)
                {
                    FirebaseAuthManager.Instance.SetGoogleSession(googleUserId, cloudData.email ?? "", googleUsername, googleToken);
                }

                PlayerPrefs.SetInt("HasGuestSave", 0);
                PlayerPrefs.DeleteKey("GuestUserId");
                PlayerPrefs.Save();

                // Apply the saved cloud data to the game
                FirebaseDatabaseManager.Instance.ApplyDataToGame(cloudData, clearExisting: true);
                UpdateGoogleConnectUI();
                NotificationBannerUI.Show("Cloud Save Loaded!", $"Welcome back, {googleUsername}! Restored your saved game.");

                AutoSaveManager.IsAutoSavePaused = false;

                // Transition to the player's saved scene
                string targetScene = !string.IsNullOrEmpty(cloudData.currentScene) ? cloudData.currentScene : "Lobby";
                Time.timeScale = 1f;
                SceneManager.LoadScene(targetScene);
            });

            // Button 2: Cancel (Stay as Guest)
            var cancelBtnGo = CreateWoodenModalButton(frameGo.transform, "Button_StayGuest", "STAY AS GUEST",
                new Vector2(130f, -148f), new Vector2(235f, 62f), sprBtnNormal, sprBtnHover, new Color(0.96f, 0.92f, 0.86f, 1f));
            var cancelBtn = cancelBtnGo.GetComponent<Button>();
            cancelBtn.onClick.AddListener(() =>
            {
                if (_accountConflictModal != null) Destroy(_accountConflictModal);

                // Revert back to the guest session so cloud save is preserved untouched
                FirebaseAuthManager.Instance.RestoreGuestSession(prevGuestId, prevGuestName);
                PlayerPrefs.SetInt("HasGuestSave", 1);
                PlayerPrefs.Save();
                UpdateGoogleConnectUI();
                if (settingsPanel != null) settingsPanel.SetActive(true);
                AutoSaveManager.IsAutoSavePaused = false;
                NotificationBannerUI.Show("Account Already Registered", "Cloud save preserved. Kept playing as Guest.");
            });
        }

        private GameObject CreateWoodenModalButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Sprite sprNormal, Sprite sprHover, Color textColor)
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
            img.sprite = sprNormal;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.raycastTarget = true;

            var btn = btnGo.GetComponent<Button>();
            if (sprHover != null)
            {
                btn.transition = Selectable.Transition.SpriteSwap;
                var state = btn.spriteState;
                state.highlightedSprite = sprHover;
                state.pressedSprite = sprNormal;
                btn.spriteState = state;
            }

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            var tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 14.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;

            var shadow = txtGo.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.90f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

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
            AutoSaveManager.IsAutoSavePaused = false;

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
                GameProgression.UnlockBook();
                UpdateBookButtonState();
            }

            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
                Time.timeScale = 1f;
                SetPlayerMovementState(true);
            }

            if (bagModal == null)
            {
                bagModal = FindFirstObjectByType<BagModalUI>(FindObjectsInactive.Include);
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
