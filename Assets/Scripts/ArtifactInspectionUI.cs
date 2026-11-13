using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    /// <summary>
    /// UI Controller for the Artifact Inspection modal with dialogue box,
    /// dynamic character avatar, and choice prompts ("inspect artifact" / "inspect plaque" / "Place from bag").
    /// Allows the player to open their bag and select which recovered artifact to restore to the pedestal.
    /// </summary>
    public class ArtifactInspectionUI : MonoBehaviour
    {
        public static ArtifactInspectionUI Instance { get; private set; }

        [Header("Root")]
        [SerializeField] private GameObject rootPanel;

        [Header("Character Info & Avatar")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private Sprite boyAvatarSprite;
        [SerializeField] private Sprite girlAvatarSprite;
        [SerializeField] private TMP_Text characterNameText;
        [SerializeField] private TMP_Text dialogueText;

        [Header("Choice Container & Buttons")]
        [SerializeField] private GameObject choiceContainer;
        [SerializeField] private Button inspectArtifactButton;
        [SerializeField] private Button inspectPlaqueButton;
        [SerializeField] private Button restoreArtifactButton;

        [Header("Plaque Popup")]
        [SerializeField] private GameObject plaquePopup;
        [SerializeField] private TMP_Text plaqueTitleText;
        [SerializeField] private TMP_Text plaqueBodyText;
        [SerializeField] private Button closePlaqueButton;

        [Header("Artifact Detail Popup")]
        [SerializeField] private GameObject artifactPopup;
        [SerializeField] private Image artifactGlowImage;
        [SerializeField] private Image artifactDetailImage;
        [SerializeField] private TMP_Text artifactDetailTitleText;
        [SerializeField] private TMP_Text artifactDetailBodyText;
        [SerializeField] private Button closeArtifactDetailButton;

        [Header("Close All / Background Tap")]
        [SerializeField] private Button backgroundTapButton;
        [SerializeField] private Button dialogueArrowButton;

        [Header("Dialogue Box Sprites")]
        [SerializeField] private Sprite standardDialogueBoxSprite;
        [SerializeField] private Sprite longDialogueBoxSprite;

        private Image _dialogueBoxImage;
        private RectTransform _dialogueBoxRt;
        private RectTransform _characterNameRt;
        private RectTransform _dialogueTextRt;
        private RectTransform _dialogueArrowRt;
        private bool _isLongDialogueMode = false;

        private ArtifactInteractable _currentArtifact;
        private PlayerMovement _playerMovement;
        private bool _isOpen = false;
        public bool IsOpen => _isOpen;
        private readonly List<GameObject> _hiddenHudObjects = new List<GameObject>();

        // Restore from Bag Picker Modal
        private GameObject _restorePickerModal;
        private Transform _restoreItemsGrid;
        private TMP_Text _restoreEmptyText;
        private readonly List<GameObject> _spawnedRestoreCards = new List<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (inspectArtifactButton != null)
                inspectArtifactButton.onClick.AddListener(OnInspectArtifactClicked);

            if (inspectPlaqueButton != null)
                inspectPlaqueButton.onClick.AddListener(OnInspectPlaqueClicked);

            EnsureRestoreButton();
            if (restoreArtifactButton != null)
                restoreArtifactButton.onClick.AddListener(OnRestoreArtifactClicked);

            if (closePlaqueButton != null)
                closePlaqueButton.onClick.AddListener(OnDialogueArrowClicked);

            if (closeArtifactDetailButton != null)
                closeArtifactDetailButton.onClick.AddListener(OnDialogueArrowClicked);

            if (backgroundTapButton != null)
                backgroundTapButton.onClick.AddListener(Close);

            EnsureDialogueArrowButton();
            EnsureDialogueBoxReferences();
            ApplyDialogueBoxMode(false);

            if (rootPanel != null)
                rootPanel.SetActive(false);

            if (_playerMovement == null)
                _playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (_playerMovement != null)
                _playerMovement.enabled = true;
        }

        private void EnsureDialogueBoxReferences()
        {
            if (_dialogueBoxRt == null)
            {
                Transform dbox = null;
                if (dialogueText != null)
                {
                    dbox = dialogueText.transform.parent;
                }
                if (dbox == null && rootPanel != null)
                {
                    dbox = rootPanel.transform.Find("DialogueBox");
                }
                if (dbox != null)
                {
                    _dialogueBoxRt = dbox.GetComponent<RectTransform>();
                    _dialogueBoxImage = dbox.GetComponent<Image>();

                    var nameTr = dbox.Find("CharacterName");
                    if (nameTr != null) _characterNameRt = nameTr.GetComponent<RectTransform>();

                    var arrowTr = dbox.Find("DialogueArrowButton");
                    if (arrowTr != null) _dialogueArrowRt = arrowTr.GetComponent<RectTransform>();
                }
            }
            if (_dialogueTextRt == null && dialogueText != null)
            {
                _dialogueTextRt = dialogueText.GetComponent<RectTransform>();
            }
            if (_characterNameRt == null && characterNameText != null)
            {
                _characterNameRt = characterNameText.GetComponent<RectTransform>();
            }
            if (_dialogueArrowRt == null && dialogueArrowButton != null)
            {
                _dialogueArrowRt = dialogueArrowButton.GetComponent<RectTransform>();
            }

            if (standardDialogueBoxSprite == null)
            {
#if UNITY_EDITOR
                standardDialogueBoxSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/DialogueBox_Clean.png");
                if (standardDialogueBoxSprite == null)
                    standardDialogueBoxSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/DialogueBox.png");
#endif
            }

            if (longDialogueBoxSprite == null)
            {
#if UNITY_EDITOR
                longDialogueBoxSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/DialogueBox_Long.png");
#endif
            }
        }

        public void ApplyDialogueBoxMode(bool isLongMode)
        {
            EnsureDialogueBoxReferences();
            if (_dialogueBoxRt == null) return;

            _isLongDialogueMode = isLongMode;

            if (isLongMode)
            {
                if (longDialogueBoxSprite != null && _dialogueBoxImage != null)
                    _dialogueBoxImage.sprite = longDialogueBoxSprite;

                _dialogueBoxRt.anchoredPosition = new Vector2(145f, 35f);
                _dialogueBoxRt.sizeDelta = new Vector2(1380f, 151f);

                if (_characterNameRt != null)
                {
                    _characterNameRt.anchoredPosition = new Vector2(82f, -8f);
                    _characterNameRt.sizeDelta = new Vector2(160f, 26f);
                }
                if (characterNameText != null)
                {
                    characterNameText.fontSize = 21f;
                }

                if (_dialogueTextRt != null)
                {
                    _dialogueTextRt.anchorMin = new Vector2(0f, 0f);
                    _dialogueTextRt.anchorMax = new Vector2(1f, 1f);
                    _dialogueTextRt.offsetMin = new Vector2(40f, 18f);
                    _dialogueTextRt.offsetMax = new Vector2(-75f, -38f);
                }

                if (dialogueText != null)
                {
                    dialogueText.fontSize = 28f;
                    dialogueText.enableAutoSizing = true;
                    dialogueText.fontSizeMin = 22f;
                    dialogueText.fontSizeMax = 28f;
                }

                if (_dialogueArrowRt != null)
                {
                    _dialogueArrowRt.anchoredPosition = new Vector2(-45f, 20f);
                    _dialogueArrowRt.sizeDelta = new Vector2(50f, 35f);
                }
            }
            else
            {
                if (standardDialogueBoxSprite != null && _dialogueBoxImage != null)
                    _dialogueBoxImage.sprite = standardDialogueBoxSprite;

                _dialogueBoxRt.anchoredPosition = new Vector2(110f, 35f);
                _dialogueBoxRt.sizeDelta = new Vector2(1050f, 287f);

                if (_characterNameRt != null)
                {
                    _characterNameRt.anchoredPosition = new Vector2(120f, -30f);
                    _characterNameRt.sizeDelta = new Vector2(230f, 45f);
                }
                if (characterNameText != null)
                {
                    characterNameText.fontSize = 28f;
                }

                if (_dialogueTextRt != null)
                {
                    _dialogueTextRt.anchorMin = new Vector2(0f, 0f);
                    _dialogueTextRt.anchorMax = new Vector2(1f, 1f);
                    _dialogueTextRt.offsetMin = new Vector2(55f, 30f);
                    _dialogueTextRt.offsetMax = new Vector2(-110f, -100f);
                }

                if (dialogueText != null)
                {
                    dialogueText.fontSize = 28f;
                    dialogueText.enableAutoSizing = true;
                    dialogueText.fontSizeMin = 22f;
                    dialogueText.fontSizeMax = 28f;
                }

                if (_dialogueArrowRt != null)
                {
                    _dialogueArrowRt.anchoredPosition = new Vector2(-67f, 41.5f);
                    _dialogueArrowRt.sizeDelta = new Vector2(85f, 75f);
                }
            }
        }

        public void SetDialogueMessage(string message)
        {
            EnsureDialogueBoxReferences();
            if (dialogueText != null)
            {
                dialogueText.text = message;
                dialogueText.color = new Color(0.18f, 0.12f, 0.08f, 1f);
            }

            bool choicesActive = (choiceContainer != null && choiceContainer.activeSelf);
            bool shouldBeLong = !choicesActive && !string.IsNullOrEmpty(message) && message.Length >= 120;

            ApplyDialogueBoxMode(shouldBeLong);
        }

        private void CleanAllChoiceButtons()
        {
            if (choiceContainer == null) return;
            foreach (Transform child in choiceContainer.transform)
            {
                if (child == null) continue;
                var badge = child.Find("Badge");
                if (badge != null)
                {
                    if (Application.isPlaying) Destroy(badge.gameObject);
                    else DestroyImmediate(badge.gameObject);
                }
                var letter = child.Find("Letter");
                if (letter != null)
                {
                    if (Application.isPlaying) Destroy(letter.gameObject);
                    else DestroyImmediate(letter.gameObject);
                }
                var labelTr = child.Find("Label");
                if (labelTr != null)
                {
                    var rt = labelTr.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.offsetMin = new Vector2(20f, 0f);
                        rt.offsetMax = new Vector2(-20f, 0f);
                    }
                    var tmp = labelTr.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.fontSize = 26f;
                        tmp.fontStyle = FontStyles.Bold;
                    }
                }
            }
        }

        private void EnsureRestoreButton()
        {
            if (choiceContainer == null) return;

            CleanAllChoiceButtons();

            if (restoreArtifactButton == null)
            {
                var existing = choiceContainer.transform.Find("RestoreArtifactButton");
                if (existing != null)
                {
                    restoreArtifactButton = existing.GetComponent<Button>();
                }
                else if (inspectArtifactButton != null)
                {
                    var btnObj = Instantiate(inspectArtifactButton.gameObject, choiceContainer.transform);
                    btnObj.name = "RestoreArtifactButton";
                    var txt = btnObj.GetComponentInChildren<TMP_Text>();
                    if (txt != null)
                    {
                        txt.text = "Place from bag";
                        txt.alignment = TextAlignmentOptions.Center;
                    }
                    restoreArtifactButton = btnObj.GetComponent<Button>();
                    restoreArtifactButton.onClick.RemoveAllListeners();
                    restoreArtifactButton.onClick.AddListener(OnRestoreArtifactClicked);

                    // Configure layout
                    var vlg = choiceContainer.GetComponent<VerticalLayoutGroup>();
                    if (vlg == null)
                    {
                        vlg = choiceContainer.AddComponent<VerticalLayoutGroup>();
                        vlg.childAlignment = TextAnchor.MiddleCenter;
                        vlg.spacing = 10f;
                        vlg.childControlWidth = false;
                        vlg.childControlHeight = false;
                        vlg.childForceExpandWidth = false;
                        vlg.childForceExpandHeight = false;

                        var rt = choiceContainer.GetComponent<RectTransform>();
                        if (rt != null) rt.sizeDelta = new Vector2(360f, 220f);
                    }

                    btnObj.transform.SetSiblingIndex(0); // Place at top of choice box
                }
            }

            CleanAllChoiceButtons();

            if (restoreArtifactButton != null)
            {
                var txt = restoreArtifactButton.GetComponentInChildren<TMP_Text>();
                if (txt != null)
                {
                    txt.text = "Place from bag";
                }
                restoreArtifactButton.gameObject.SetActive(true);
            }
        }

        public void TriggerAdvanceDialogue()
        {
            if (!_isOpen) return;
            if ((plaquePopup != null && plaquePopup.activeSelf) || (artifactPopup != null && artifactPopup.activeSelf))
            {
                OnDialogueArrowClicked();
            }
        }

        private void Update()
        {
            if (!_isOpen) return;

            if (MobileInteractButton.WasPressedThisFrame)
            {
                TriggerAdvanceDialogue();
            }

            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb.escapeKey.wasPressedThisFrame)
                {
                    OnDialogueArrowClicked();
                    return;
                }

                if (choiceContainer != null && choiceContainer.activeSelf)
                {
                    if ((kb.digit1Key.wasPressedThisFrame || kb.rKey.wasPressedThisFrame) &&
                        restoreArtifactButton != null && restoreArtifactButton.gameObject.activeSelf)
                    {
                        OnRestoreArtifactClicked();
                    }
                    else if (kb.digit2Key.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
                    {
                        OnInspectArtifactClicked();
                    }
                    else if (kb.digit3Key.wasPressedThisFrame || kb.pKey.wasPressedThisFrame)
                    {
                        OnInspectPlaqueClicked();
                    }
                }
                else if ((plaquePopup != null && plaquePopup.activeSelf) || (artifactPopup != null && artifactPopup.activeSelf))
                {
                    if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
                    {
                        OnDialogueArrowClicked();
                    }
                }
            }
        }

        private bool CheckAndUnlockRetryForUnobtainedArtifact(ArtifactInteractable artifact)
        {
            if (artifact == null) return false;
            int rNum = artifact.RoomNumber;

            // An item is unobtained if not restored on pedestal and not in bag
            bool isRestored = artifact.IsRestored;
            bool inBag = DoesPlayerHaveArtifactForPedestal(artifact);
            if (isRestored || inBag) return false;

            // Check if room battle was previously attempted or completed
            bool hasAttemptedOrCompleted = GameProgression.HasRetrievedRoomArtifact(rNum) || 
                                           GameProgression.IsRoomBattleCompleted(rNum) ||
                                           GameProgression.GetRoomRetrievedCount(rNum) > 0;

            if (hasAttemptedOrCompleted && GameProgression.CanRoomBeRetried(rNum))
            {
                bool wasLocked = GameProgression.UnlockRoomBattleForRetry(rNum);
                if (wasLocked)
                {
                    NotificationBannerUI.Show(
                        "TRIAL UNLOCKED FOR RETRY",
                        $"Alterworld Battle {rNum} is now unlocked! Challenge it again to retrieve '{artifact.ArtifactName}'.",
                        5.5f,
                        new Color(0.18f, 0.40f, 0.16f, 1f),
                        "★"
                    );
                }
                return true;
            }

            return false;
        }

        public void Open(ArtifactInteractable artifact)
        {
            if (Instance == null) Instance = this;
            _currentArtifact = artifact;
            _isOpen = true;

            // Pause player movement
            if (_playerMovement == null)
                _playerMovement = FindFirstObjectByType<PlayerMovement>();

            if (_playerMovement != null)
            {
                _playerMovement.enabled = false;
                var rb = _playerMovement.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }

            // Hide other gameplay HUD elements (Joystick, Hearts, Room Banner, Home Button)
            _hiddenHudObjects.Clear();
            var canvasTr = rootPanel != null ? rootPanel.transform.parent : transform;
            if (canvasTr != null)
            {
                foreach (Transform child in canvasTr)
                {
                    if (child.gameObject != rootPanel &&
                        child.gameObject.name != "NotificationBanner" &&
                        child.GetComponent<NotificationBannerUI>() == null &&
                        child.gameObject.activeSelf)
                    {
                        _hiddenHudObjects.Add(child.gameObject);
                        child.gameObject.SetActive(false);
                    }
                }
            }

            // Update Avatar based on character selection
            bool isGirl = false;
            var anim = FindFirstObjectByType<CharacterAnimator2D>();
            if (anim != null)
            {
                isGirl = anim.IsGirlActive;
            }
            else
            {
                isGirl = string.Equals(PlayerPrefs.GetString("SelectedCharacter", "Boy"), "Girl", StringComparison.OrdinalIgnoreCase);
            }

            if (avatarImage != null)
            {
                avatarImage.sprite = isGirl ? girlAvatarSprite : boyAvatarSprite;
            }

            if (characterNameText != null)
            {
                string defaultName = isGirl ? "Darla" : "Darrel";
                characterNameText.text = PlayerPrefs.GetString("CustomCharacterName", defaultName);
                characterNameText.color = new Color(1f, 0.95f, 0.85f, 1f);
            }

            int rNum = artifact != null ? artifact.RoomNumber : 1;
            bool isPedestalRestored = artifact != null && artifact.IsRestored;
            bool allRead = GameProgression.AreAllPlaquesRead(rNum);
            int readCount = GameProgression.GetPlaquesReadCount(rNum);
            bool hasMatchingInBag = DoesPlayerHaveArtifactForPedestal(artifact);
            bool retryUnlocked = CheckAndUnlockRetryForUnobtainedArtifact(artifact);

            UpdateChoiceButtons();

            string initMsg = "";
            if (isPedestalRestored)
            {
                initMsg = $"The {artifact.ArtifactName} has been safely restored to this pedestal!";
            }
            else if (hasMatchingInBag)
            {
                initMsg = "I have a matching artifact in my bag! I should place it on this pedestal.";
            }
            else if (retryUnlocked)
            {
                initMsg = $"The {artifact.ArtifactName} hasn't been recovered yet! By inspecting this pedestal, Alterworld Battle {rNum} is now unlocked for retry. I can enter the portal to retrieve it!";
            }
            else if (allRead)
            {
                if (rNum == 1)
                    initMsg = "I have studied all 5 plaques in Room 1! The mysterious portal in the museum lobby is now awakened!";
                else
                    initMsg = $"I have studied all 5 plaques in Room {rNum}! The Alterworld trial for Room {rNum} is now unlocked!";
            }
            else
            {
                initMsg = $"I need to study all 5 historical plaques in this room ({readCount}/5 studied). What should I do?";
            }
            SetDialogueMessage(initMsg);

            EnsureDialogueArrowButton();

            if (rootPanel != null)
                rootPanel.SetActive(true);
        }

        private void UpdateChoiceButtons()
        {
            if (choiceContainer != null)
            {
                var choiceRt = choiceContainer.GetComponent<RectTransform>();
                if (choiceRt != null)
                {
                    choiceRt.anchorMin = new Vector2(1f, 0f);
                    choiceRt.anchorMax = new Vector2(1f, 0f);
                    choiceRt.pivot = new Vector2(1f, 0f);
                    choiceRt.anchoredPosition = new Vector2(-65f, 335f);
                    choiceRt.sizeDelta = new Vector2(360f, 220f);
                }
                choiceContainer.SetActive(true);
            }
            if (plaquePopup != null) plaquePopup.SetActive(false);
            if (artifactPopup != null) artifactPopup.SetActive(false);
            if (_restorePickerModal != null) _restorePickerModal.SetActive(false);

            EnsureRestoreButton();
            if (restoreArtifactButton != null)
            {
                // Option "Place from bag" is always visible on all artifacts
                restoreArtifactButton.gameObject.SetActive(true);
            }
            if (inspectArtifactButton != null) inspectArtifactButton.gameObject.SetActive(true);
            if (inspectPlaqueButton != null) inspectPlaqueButton.gameObject.SetActive(true);

            EnsureDialogueArrowButton();
            CleanAllChoiceButtons();
            ApplyDialogueBoxMode(false);
        }

        public void OnDialogueArrowClicked()
        {
            if (_restorePickerModal != null && _restorePickerModal.activeSelf)
            {
                CloseRestorePicker();
                return;
            }

            if ((artifactPopup != null && artifactPopup.activeSelf) || (plaquePopup != null && plaquePopup.activeSelf))
            {
                ReturnToChoiceMenu();
                return;
            }

            Close();
        }

        public void ReturnToChoiceMenu()
        {
            if (plaquePopup != null) plaquePopup.SetActive(false);
            if (artifactPopup != null) artifactPopup.SetActive(false);
            if (_restorePickerModal != null) _restorePickerModal.SetActive(false);

            UpdateChoiceButtons();

            if (_currentArtifact != null)
            {
                int rNum = _currentArtifact.RoomNumber;
                bool isPedestalRestored = _currentArtifact.IsRestored;
                bool allRead = GameProgression.AreAllPlaquesRead(rNum);
                int readCount = GameProgression.GetPlaquesReadCount(rNum);
                bool hasMatchingInBag = DoesPlayerHaveArtifactForPedestal(_currentArtifact);
                bool battleDone = GameProgression.HasRetrievedRoomArtifact(rNum);

                string menuMsg = "";
                if (isPedestalRestored)
                {
                    menuMsg = $"The {_currentArtifact.ArtifactName} has been safely restored to this pedestal!";
                }
                else if (hasMatchingInBag)
                {
                    menuMsg = "I have a matching artifact in my bag! I should place it on this pedestal.";
                }
                else if (battleDone)
                {
                    menuMsg = "This pedestal is empty. The artifact was lost in the Alterworld battle and was not recovered.";
                }
                else if (allRead)
                {
                    if (rNum == 1)
                        menuMsg = "I have studied all 5 plaques in Room 1! The mysterious portal in the museum lobby is now awakened!";
                    else
                        menuMsg = $"I have studied all 5 plaques in Room {rNum}! The Alterworld trial for Room {rNum} is now unlocked!";
                }
                else
                {
                    menuMsg = $"I need to study all 5 historical plaques in this room ({readCount}/5 studied). What should I do?";
                }
                SetDialogueMessage(menuMsg);
            }
        }

        private void EnsureDialogueArrowButton()
        {
            if (dialogueArrowButton == null)
            {
                Transform dbox = dialogueText != null ? dialogueText.transform.parent : null;
                if (dbox == null && rootPanel != null)
                {
                    dbox = rootPanel.transform.Find("DialogueBox");
                }
                if (dbox == null)
                {
                    var dboxes = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (var go in dboxes)
                    {
                        if (go.name == "DialogueBox" && (rootPanel == null || go.transform.IsChildOf(rootPanel.transform)))
                        {
                            dbox = go.transform;
                            break;
                        }
                    }
                }

                if (dbox != null)
                {
                    var existing = dbox.Find("DialogueArrowButton");
                    if (existing != null)
                    {
                        dialogueArrowButton = existing.GetComponent<Button>();
                    }
                    else
                    {
                        var btnObj = new GameObject("DialogueArrowButton", typeof(RectTransform), typeof(Image), typeof(Button));
                        btnObj.transform.SetParent(dbox, false);

                        var rt = btnObj.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(1f, 0f);
                        rt.anchorMax = new Vector2(1f, 0f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = new Vector2(-45f, 20f);
                        rt.sizeDelta = new Vector2(50f, 35f);

                        var img = btnObj.GetComponent<Image>();
                        img.color = new Color(1f, 1f, 1f, 0.001f);
                        img.raycastTarget = true;

                        dialogueArrowButton = btnObj.GetComponent<Button>();
                        dialogueArrowButton.targetGraphic = img;

                        var colors = dialogueArrowButton.colors;
                        colors.normalColor = new Color(1f, 1f, 1f, 0.001f);
                        colors.highlightedColor = new Color(1f, 0.85f, 0.3f, 0.35f);
                        colors.pressedColor = new Color(0.9f, 0.6f, 0.1f, 0.55f);
                        colors.selectedColor = new Color(1f, 0.85f, 0.3f, 0.35f);
                        dialogueArrowButton.colors = colors;
                    }
                }
            }

            if (dialogueArrowButton != null)
            {
                dialogueArrowButton.onClick.RemoveListener(OnDialogueArrowClicked);
                dialogueArrowButton.onClick.AddListener(OnDialogueArrowClicked);
                dialogueArrowButton.gameObject.SetActive(true);
            }
        }

        private bool DoesPlayerHaveArtifactForPedestal(ArtifactInteractable pedestal)
        {
            if (pedestal == null) return false;
            foreach (var item in PlayerInventory.Items)
            {
                if (DoesItemMatchPedestal(item, pedestal)) return true;
            }
            return false;
        }

        public static bool DoesItemMatchPedestal(ArtifactItemData item, ArtifactInteractable pedestal)
        {
            if (item == null || pedestal == null) return false;

            // 1. Direct name match
            if (string.Equals(item.name, pedestal.ArtifactName, StringComparison.OrdinalIgnoreCase))
                return true;

            // 2. ID match
            if (!string.IsNullOrEmpty(item.id))
            {
                if (string.Equals(item.id, pedestal.ArtifactName, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (pedestal.gameObject.name.Contains(item.id) || item.id.Contains(pedestal.gameObject.name))
                    return true;
            }

            // 3. Normalized keyword match
            string iname = item.name.ToLowerInvariant();
            string pname = pedestal.ArtifactName.ToLowerInvariant();
            if (iname.Contains("seal") && pname.Contains("seal")) return true;
            if ((iname.Contains("notebook") || iname.Contains("manuscript") || iname.Contains("book")) && (pname.Contains("notebook") || pname.Contains("manuscript") || pname.Contains("book"))) return true;
            if ((iname.Contains("quill") || iname.Contains("nib") || iname.Contains("pen")) && (pname.Contains("quill") || pname.Contains("nib") || pname.Contains("pen"))) return true;
            if ((iname.Contains("spectacles") || iname.Contains("glasses")) && (pname.Contains("spectacles") || pname.Contains("glasses"))) return true;
            if (iname.Contains("locket") && pname.Contains("locket")) return true;

            return false;
        }

        public void OnRestoreArtifactClicked()
        {
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (plaquePopup != null) plaquePopup.SetActive(false);
            if (artifactPopup != null) artifactPopup.SetActive(false);

            if (_currentArtifact != null && _currentArtifact.IsRestored)
            {
                NotificationBannerUI.Show(
                    "ALREADY RESTORED",
                    $"The {_currentArtifact.ArtifactName} is already safely restored to this pedestal!",
                    3.5f,
                    new Color(0.2f, 0.95f, 0.4f, 1f),
                    "★"
                );
                SetDialogueMessage($"The {_currentArtifact.ArtifactName} has already been safely restored to this pedestal!");
                if (choiceContainer != null) choiceContainer.SetActive(true);
                return;
            }

            EnsureRestorePickerModal();
            RefreshRestorePicker();
            if (_restorePickerModal != null) _restorePickerModal.SetActive(true);
        }

        public void CloseRestorePicker()
        {
            if (_restorePickerModal != null) _restorePickerModal.SetActive(false);
            UpdateChoiceButtons();
        }

        private void EnsureRestorePickerModal()
        {
            if (_restorePickerModal != null) return;

            var canvasTr = rootPanel != null ? rootPanel.transform : transform;
            var modalObj = new GameObject("RestorePickerModal", typeof(RectTransform));
            modalObj.transform.SetParent(canvasTr, false);
            var modalRt = modalObj.GetComponent<RectTransform>();
            modalRt.anchorMin = Vector2.zero;
            modalRt.anchorMax = Vector2.one;
            modalRt.offsetMin = Vector2.zero;
            modalRt.offsetMax = Vector2.zero;

            // Fullscreen Dimmer
            var dimmerObj = new GameObject("Dimmer", typeof(RectTransform), typeof(Image), typeof(Button));
            dimmerObj.transform.SetParent(modalObj.transform, false);
            var dimmerRt = dimmerObj.GetComponent<RectTransform>();
            dimmerRt.anchorMin = Vector2.zero;
            dimmerRt.anchorMax = Vector2.one;
            dimmerRt.offsetMin = Vector2.zero;
            dimmerRt.offsetMax = Vector2.zero;
            var dimmerImg = dimmerObj.GetComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.75f);
            var dimmerBtn = dimmerObj.GetComponent<Button>();
            dimmerBtn.transition = Selectable.Transition.None;
            dimmerBtn.onClick.AddListener(CloseRestorePicker);

            // Window
            var windowObj = new GameObject("Window", typeof(RectTransform), typeof(Image));
            windowObj.transform.SetParent(modalObj.transform, false);
            var windowRt = windowObj.GetComponent<RectTransform>();
            windowRt.anchorMin = new Vector2(0.5f, 0.5f);
            windowRt.anchorMax = new Vector2(0.5f, 0.5f);
            windowRt.pivot = new Vector2(0.5f, 0.5f);
            windowRt.anchoredPosition = Vector2.zero;
            windowRt.sizeDelta = new Vector2(899f, 492f);
            var winImg = windowObj.GetComponent<Image>();
            winImg.color = Color.white;
            winImg.type = Image.Type.Simple;
            var sprSel = Resources.Load<Sprite>("UI/SelectArtifact_Frame");
#if UNITY_EDITOR
            if (sprSel == null) sprSel = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/SelectArtifact_Frame.png");
#endif
            if (sprSel != null) winImg.sprite = sprSel;

            // Title & Subtitle are baked into the flap art of SelectArtifact_Frame, so we keep them hidden
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(windowObj.transform, false);
            titleObj.SetActive(false);

            var subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subObj.transform.SetParent(windowObj.transform, false);
            subObj.SetActive(false);

            // Grid Container
            var gridObj = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObj.transform.SetParent(windowObj.transform, false);
            var gridRt = gridObj.GetComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0.5f, 0.5f);
            gridRt.anchorMax = new Vector2(0.5f, 0.5f);
            gridRt.pivot = new Vector2(0.5f, 0.5f);
            gridRt.anchoredPosition = new Vector2(0f, -36f);
            gridRt.sizeDelta = new Vector2(790f, 325f);
            var glg = gridObj.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(382f, 95f);
            glg.spacing = new Vector2(16f, 12f);
            glg.padding = new RectOffset(0, 0, 0, 0);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;
            glg.childAlignment = TextAnchor.UpperLeft;

            // Empty Notice
            var emptyNoticeObj = new GameObject("EmptyNotice", typeof(RectTransform), typeof(TextMeshProUGUI));
            emptyNoticeObj.transform.SetParent(windowObj.transform, false);
            var enRt = emptyNoticeObj.GetComponent<RectTransform>();
            enRt.anchorMin = new Vector2(0.5f, 0.5f);
            enRt.anchorMax = new Vector2(0.5f, 0.5f);
            enRt.pivot = new Vector2(0.5f, 0.5f);
            enRt.anchoredPosition = new Vector2(0f, -36f);
            enRt.sizeDelta = new Vector2(650f, 80f);
            var enTmp = emptyNoticeObj.GetComponent<TextMeshProUGUI>();
            enTmp.text = "Your bag is empty.\nYou have no recovered artifacts to place.";
            enTmp.fontSize = 20f;
            enTmp.alignment = TextAlignmentOptions.Center;
            enTmp.color = new Color(0.85f, 0.80f, 0.70f, 0.95f);

            // Close Button
            var closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(windowObj.transform, false);
            var closeRt = closeBtnObj.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-16f, -16f);
            closeRt.sizeDelta = new Vector2(34f, 34f);
            var closeImg = closeBtnObj.GetComponent<Image>();
            closeImg.color = new Color(0.72f, 0.20f, 0.18f, 0.95f);
            var closeOutline = closeBtnObj.AddComponent<Outline>();
            closeOutline.effectColor = new Color(0.35f, 0.08f, 0.06f, 0.9f);
            closeOutline.effectDistance = new Vector2(1f, -1f);
            var closeBtn = closeBtnObj.GetComponent<Button>();
            closeBtn.onClick.AddListener(CloseRestorePicker);

            var closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
            var closeTxtRt = closeTxtObj.GetComponent<RectTransform>();
            closeTxtRt.anchorMin = Vector2.zero;
            closeTxtRt.anchorMax = Vector2.one;
            closeTxtRt.offsetMin = Vector2.zero;
            closeTxtRt.offsetMax = Vector2.zero;
            var closeTmp = closeTxtObj.GetComponent<TextMeshProUGUI>();
            closeTmp.text = "X";
            closeTmp.fontSize = 20f;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.alignment = TextAlignmentOptions.Center;
            closeTmp.color = Color.white;

            _restorePickerModal = modalObj;
            _restoreItemsGrid = gridObj.transform;
            _restoreEmptyText = enTmp;

            modalObj.SetActive(false);
        }

        private void RefreshRestorePicker()
        {
            EnsureRestorePickerModal();
            if (_restoreItemsGrid == null) return;

            foreach (var c in _spawnedRestoreCards)
            {
                if (c != null) Destroy(c);
            }
            _spawnedRestoreCards.Clear();

            var items = PlayerInventory.Items;
            if (_restoreEmptyText != null)
            {
                _restoreEmptyText.gameObject.SetActive(items.Count == 0);
            }

            foreach (var item in items)
            {
                var cardObj = new GameObject($"RestoreCard_{item.id}", typeof(RectTransform), typeof(Image), typeof(Button));
                cardObj.transform.SetParent(_restoreItemsGrid, false);

                var img = cardObj.GetComponent<Image>();
                img.color = new Color(0.20f, 0.16f, 0.13f, 0.98f);

                var outline = cardObj.AddComponent<Outline>();
                outline.effectColor = new Color(0.68f, 0.52f, 0.32f, 0.85f);
                outline.effectDistance = new Vector2(1.5f, -1.5f);

                var cardRt = cardObj.GetComponent<RectTransform>();
                cardRt.sizeDelta = new Vector2(382f, 95f);

                // Icon Frame (light parchment background for high contrast)
                var iconFrameObj = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
                iconFrameObj.transform.SetParent(cardObj.transform, false);
                var iconFrameRt = iconFrameObj.GetComponent<RectTransform>();
                iconFrameRt.anchorMin = new Vector2(0f, 0.5f);
                iconFrameRt.anchorMax = new Vector2(0f, 0.5f);
                iconFrameRt.pivot = new Vector2(0f, 0.5f);
                iconFrameRt.anchoredPosition = new Vector2(10f, 0f);
                iconFrameRt.sizeDelta = new Vector2(76f, 76f);

                var frameImg = iconFrameObj.GetComponent<Image>();
                frameImg.color = new Color(0.92f, 0.88f, 0.80f, 1f);

                var frameOutline = iconFrameObj.AddComponent<Outline>();
                frameOutline.effectColor = new Color(0.45f, 0.35f, 0.22f, 0.6f);
                frameOutline.effectDistance = new Vector2(1f, -1f);

                var icBackObj = new GameObject("Backing", typeof(RectTransform), typeof(Image));
                icBackObj.transform.SetParent(iconFrameObj.transform, false);
                var icBackRt = icBackObj.GetComponent<RectTransform>();
                icBackRt.anchorMin = Vector2.zero;
                icBackRt.anchorMax = Vector2.one;
                icBackRt.offsetMin = new Vector2(3f, 3f);
                icBackRt.offsetMax = new Vector2(-3f, -3f);
                icBackObj.GetComponent<Image>().color = new Color(0.85f, 0.80f, 0.72f, 0.7f);

                // Icon
                var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(iconFrameObj.transform, false);
                var iconRt = iconObj.GetComponent<RectTransform>();
                iconRt.anchorMin = Vector2.zero;
                iconRt.anchorMax = Vector2.one;
                iconRt.offsetMin = new Vector2(5f, 5f);
                iconRt.offsetMax = new Vector2(-5f, -5f);
                var iconImg = iconObj.GetComponent<Image>();
                var itemSpr = item.icon;
#if UNITY_EDITOR
                if (itemSpr == null && !string.IsNullOrEmpty(item.name))
                {
                    string lower = item.name.ToLowerInvariant();
                    string path = null;
                    if (lower.Contains("spectacles") || lower.Contains("glasses")) path = "Assets/Sprites/Artifacts/Artifact_Glasses.png";
                    else if (lower.Contains("stamp") || lower.Contains("seal")) path = "Assets/Sprites/Artifacts/Artifact_Stamp.png";
                    else if (lower.Contains("pen") || lower.Contains("nib") || lower.Contains("quill")) path = "Assets/Sprites/Artifacts/Artifact_PenNib.png";
                    else if (lower.Contains("notebook") || lower.Contains("book") || lower.Contains("journal")) path = "Assets/Sprites/Artifacts/Artifact_Book.png";
                    else if (lower.Contains("locket") || lower.Contains("pendant")) path = "Assets/Sprites/Artifacts/Artifact_Locket.png";
                    else if (lower.Contains("galley") || lower.Contains("proof")) path = "Assets/Sprites/Artifacts/Artifact_GalleyProofSheets.png";
                    else if (lower.Contains("matrix") || lower.Contains("press")) path = "Assets/Sprites/Artifacts/Artifact_PrintingPressMatrix.png";
                    else if (lower.Contains("fili") || lower.Contains("novel") || lower.Contains("edition")) path = "Assets/Sprites/Artifacts/Artifact_FirstEditionNovelCopy.png";
                    else if (lower.Contains("stove")) path = "Assets/Sprites/Artifacts/Artifact_AlcoholCookingStove.png";
                    if (path != null) itemSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
#endif
                if (itemSpr != null)
                {
                    iconImg.sprite = itemSpr;
                    iconImg.color = Color.white;
                }
                else
                {
                    iconImg.color = new Color(0.35f, 0.30f, 0.25f, 0.3f);
                }
                iconImg.preserveAspect = true;

                // Name (spacious and clear, leaving room for Place button - description removed completely)
                var nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
                nameObj.transform.SetParent(cardObj.transform, false);
                var nameRt = nameObj.GetComponent<RectTransform>();
                nameRt.anchorMin = Vector2.zero;
                nameRt.anchorMax = Vector2.one;
                nameRt.offsetMin = new Vector2(98f, 6f);
                nameRt.offsetMax = new Vector2(-128f, -6f);
                var nameTmp = nameObj.GetComponent<TextMeshProUGUI>();
                nameTmp.text = item.name;
                nameTmp.fontSize = 19f;
                nameTmp.fontStyle = FontStyles.Bold;
                nameTmp.color = new Color(1f, 0.93f, 0.75f, 1f);
                nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
                nameTmp.enableAutoSizing = true;
                nameTmp.fontSizeMin = 14f;
                nameTmp.fontSizeMax = 20f;
                nameTmp.textWrappingMode = TextWrappingModes.Normal;

                // Wooden "Place" Button
                var placeBtnObj = new GameObject("PlaceButton", typeof(RectTransform), typeof(Image), typeof(Button));
                placeBtnObj.transform.SetParent(cardObj.transform, false);
                var placeRt = placeBtnObj.GetComponent<RectTransform>();
                placeRt.anchorMin = new Vector2(1f, 0.5f);
                placeRt.anchorMax = new Vector2(1f, 0.5f);
                placeRt.pivot = new Vector2(1f, 0.5f);
                placeRt.anchoredPosition = new Vector2(-10f, 0f);
                placeRt.sizeDelta = new Vector2(112f, 35f);

                var sprBtnNorm = Resources.Load<Sprite>("UI/Button_Place_Normal");
                var sprBtnHov = Resources.Load<Sprite>("UI/Button_Place_Hover");
#if UNITY_EDITOR
                if (sprBtnNorm == null) sprBtnNorm = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Button_Place_Normal.png");
                if (sprBtnHov == null) sprBtnHov = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Button_Place_Hover.png");
#endif
                var placeImg = placeBtnObj.GetComponent<Image>();
                placeImg.sprite = sprBtnNorm;
                placeImg.type = Image.Type.Simple;
                placeImg.color = Color.white;

                var placeBtn = placeBtnObj.GetComponent<Button>();
                placeBtn.transition = Selectable.Transition.SpriteSwap;
                var ss = new SpriteState();
                ss.highlightedSprite = sprBtnHov;
                ss.pressedSprite = sprBtnHov;
                ss.selectedSprite = sprBtnHov;
                placeBtn.spriteState = ss;

                var itemCaptured = item;
                placeBtn.onClick.AddListener(() => TryPlaceArtifact(itemCaptured));

                // Also clicking card itself attempts place
                var cardBtn = cardObj.GetComponent<Button>();
                cardBtn.onClick.AddListener(() => TryPlaceArtifact(itemCaptured));

                _spawnedRestoreCards.Add(cardObj);
            }
        }

        private void TryPlaceArtifact(ArtifactItemData item)
        {
            if (_currentArtifact == null || item == null) return;

            if (!DoesItemMatchPedestal(item, _currentArtifact))
            {
                NotificationBannerUI.Show(
                    "WRONG PEDESTAL",
                    $"'{item.name}' belongs on a different pedestal!\nCheck the plaques to find its rightful place.",
                    3.5f,
                    new Color(0.95f, 0.45f, 0.25f, 1f),
                    "!"
                );
                SetDialogueMessage($"Hmm, '{item.name}' doesn't seem to belong on this pedestal. Let me check the other pedestals in this room.");
                return;
            }

            // Matching artifact!
            PlayerInventory.RemoveArtifact(item.id);
            int rNum = _currentArtifact.RoomNumber;
            GameProgression.RestorePedestal(rNum, _currentArtifact.ArtifactName);
            _currentArtifact.SetRestoredVisual(true);

            NotificationBannerUI.Show(
                "ARTIFACT RESTORED!",
                $"You restored '{item.name}' to its pedestal!",
                4.0f,
                new Color(0.2f, 0.95f, 0.4f, 1f),
                "★"
            );

            SetDialogueMessage($"I placed the {item.name} back on its pedestal where it belongs!");

            // Check room door unlock status
            int retrieved = GameProgression.GetRoomRetrievedCount(rNum);
            int restored = GameProgression.GetPedestalsRestoredCount(rNum);
            if (restored == 1 && rNum < 12)
            {
                NotificationBannerUI.Show(
                    $"ROOM {rNum + 1} UNLOCKED!",
                    $"First artifact placed on pedestal! (1/5 placed)\nReal World Room {rNum + 1} is now OPEN in the hallway!",
                    5.5f,
                    new Color(0.2f, 0.95f, 0.4f, 1f),
                    "★"
                );
            }
            else if (retrieved > 1 && restored >= retrieved)
            {
                NotificationBannerUI.Show(
                    $"ROOM {rNum} COMPLETE!",
                    $"All {retrieved} recovered artifacts restored to Room {rNum}!",
                    5.0f,
                    new Color(0.2f, 0.95f, 0.4f, 1f),
                    "★"
                );
            }

            CloseRestorePicker();
            UpdateChoiceButtons();
        }

        public void OnInspectPlaqueClicked()
        {
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (artifactPopup != null) artifactPopup.SetActive(false);
            if (_restorePickerModal != null) _restorePickerModal.SetActive(false);
            EnsureDialogueArrowButton();

            if (plaquePopup != null)
            {
                plaquePopup.SetActive(true);
                if (plaqueTitleText != null && _currentArtifact != null)
                    plaqueTitleText.text = _currentArtifact.IsRestored ? _currentArtifact.PlaqueTitle : "???";
                if (plaqueBodyText != null && _currentArtifact != null)
                    plaqueBodyText.text = _currentArtifact.PlaqueDescription;
            }

            bool retryUnlocked = CheckAndUnlockRetryForUnobtainedArtifact(_currentArtifact);

            if (_currentArtifact != null)
            {
                if (retryUnlocked)
                {
                    SetDialogueMessage($"Studying this plaque revealed clues about the missing {_currentArtifact.ArtifactName}! Alterworld Battle {_currentArtifact.RoomNumber} is unlocked for retry.");
                }
                else
                {
                    SetDialogueMessage(_currentArtifact.PlaqueDialogue);
                }
            }

            // Record plaque read and check completion
            if (_currentArtifact != null)
            {
                int rNum = _currentArtifact.RoomNumber;
                string pId = !string.IsNullOrEmpty(_currentArtifact.ArtifactName) ? _currentArtifact.ArtifactName : _currentArtifact.gameObject.name;
                bool isNew = GameProgression.RecordPlaqueRead(rNum, pId);
                int totalRead = GameProgression.GetPlaquesReadCount(rNum);

                if (totalRead >= 5)
                {
                    if (rNum == 1)
                    {
                        NotificationBannerUI.Show(
                            "PORTAL AWAKENED!",
                            "All 5 plaques in Room 1 studied!\nA mysterious portal has opened in the museum lobby!",
                            6.0f,
                            new Color(0.24f, 0.14f, 0.07f, 1f),
                            "★"
                        );
                    }
                    else
                    {
                        NotificationBannerUI.Show(
                            $"ALTERWORLD BATTLE {rNum} UNLOCKED!",
                            $"All 5 plaques & clues studied in Room {rNum}!\nEnter the Alterworld portal in the Lobby to start the battle!",
                            6.5f,
                            new Color(0.24f, 0.14f, 0.07f, 1f),
                            "★"
                        );
                    }
                }
                else
                {
                    NotificationBannerUI.Show(
                        isNew ? "PLAQUE STUDIED" : "PLAQUE REVIEWED",
                        $"Plaques studied: {totalRead}/5 in Room {rNum}",
                        3.5f,
                        new Color(0.24f, 0.14f, 0.07f, 1f),
                        "i"
                    );
                }
            }
        }

        public void OnInspectArtifactClicked()
        {
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (plaquePopup != null) plaquePopup.SetActive(false);
            if (_restorePickerModal != null) _restorePickerModal.SetActive(false);
            EnsureDialogueArrowButton();

            int rNum = _currentArtifact != null ? _currentArtifact.RoomNumber : 1;
            bool isPedestalRestored = _currentArtifact != null && _currentArtifact.IsRestored;

            if (artifactPopup != null)
            {
                artifactPopup.SetActive(true);
                if (artifactGlowImage != null)
                {
                    artifactGlowImage.gameObject.SetActive(true);
                    artifactGlowImage.color = Color.white;
                }
                if (artifactDetailImage != null && _currentArtifact != null)
                {
                    artifactDetailImage.sprite = _currentArtifact.ArtifactSprite;
                    // If pedestal is restored, display in full vibrant color! Otherwise solid black silhouette
                    if (isPedestalRestored)
                    {
                        artifactDetailImage.color = Color.white;
                    }
                    else
                    {
                        artifactDetailImage.color = Color.black; // Silhouette
                    }
                }
                if (artifactDetailTitleText != null)
                {
                    artifactDetailTitleText.gameObject.SetActive(true);
                    artifactDetailTitleText.text = isPedestalRestored ? _currentArtifact.ArtifactName : "???";
                }
                if (artifactDetailBodyText != null)
                {
                    artifactDetailBodyText.gameObject.SetActive(isPedestalRestored);
                    if (isPedestalRestored && _currentArtifact != null)
                    {
                        artifactDetailBodyText.text = _currentArtifact.ArtifactInspectionDescription;
                    }
                }
            }

            bool retryUnlocked = CheckAndUnlockRetryForUnobtainedArtifact(_currentArtifact);

            if (isPedestalRestored)
            {
                SetDialogueMessage($"The {_currentArtifact.ArtifactName} rests peacefully on its pedestal in Room {rNum}.");
            }
            else if (DoesPlayerHaveArtifactForPedestal(_currentArtifact))
            {
                SetDialogueMessage("This pedestal is missing its artifact. I have a matching artifact in my bag! I can place it here.");
            }
            else if (retryUnlocked)
            {
                SetDialogueMessage($"The {_currentArtifact.ArtifactName} has not been obtained yet. Alterworld Battle {rNum} is unlocked for retry! Enter the portal in the Lobby to challenge it again.");
            }
            else
            {
                string diag = (_currentArtifact != null && !string.IsNullOrEmpty(_currentArtifact.ArtifactDialogue) && _currentArtifact.ArtifactDialogue != "what could this artifact be")
                    ? _currentArtifact.ArtifactDialogue
                    : "i wonder what this artifact looks like";
                SetDialogueMessage(diag);
            }
        }

        public void Close()
        {
            _isOpen = false;
            ApplyDialogueBoxMode(false);
            if (rootPanel != null)
                rootPanel.SetActive(false);

            if (plaquePopup != null) plaquePopup.SetActive(false);
            if (artifactPopup != null) artifactPopup.SetActive(false);
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (_restorePickerModal != null) _restorePickerModal.SetActive(false);

            // Restore hidden gameplay HUD elements
            foreach (var hud in _hiddenHudObjects)
            {
                if (hud != null) hud.SetActive(true);
            }
            _hiddenHudObjects.Clear();

            // Re-enable player movement & ensure player sprite is enabled
            if (_playerMovement != null)
            {
                _playerMovement.enabled = true;
                var sr = _playerMovement.GetComponentInChildren<SpriteRenderer>(true);
                if (sr != null) sr.enabled = true;
            }

            _currentArtifact = null;
        }
    }
}
