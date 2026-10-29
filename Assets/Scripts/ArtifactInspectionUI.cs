using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    /// <summary>
    /// UI Controller for the Artifact Inspection modal with dialogue box,
    /// dynamic character avatar, and choice prompts ("inspect artifact" / "inspect plaque" / "place from bag").
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

        private ArtifactInteractable _currentArtifact;
        private PlayerMovement _playerMovement;
        private bool _isOpen = false;
        private readonly List<GameObject> _hiddenHudObjects = new List<GameObject>();
        private SpriteRenderer _hiddenPlayerSr;

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
                closePlaqueButton.onClick.AddListener(Close);

            if (closeArtifactDetailButton != null)
                closeArtifactDetailButton.onClick.AddListener(Close);

            if (backgroundTapButton != null)
                backgroundTapButton.onClick.AddListener(Close);

            if (rootPanel != null)
                rootPanel.SetActive(false);

            if (_playerMovement == null)
                _playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (_playerMovement != null)
                _playerMovement.enabled = true;
        }

        private void EnsureRestoreButton()
        {
            if (restoreArtifactButton != null) return;
            if (choiceContainer == null) return;

            var existing = choiceContainer.transform.Find("RestoreArtifactButton");
            if (existing != null)
            {
                restoreArtifactButton = existing.GetComponent<Button>();
                return;
            }

            if (inspectArtifactButton != null)
            {
                var btnObj = Instantiate(inspectArtifactButton.gameObject, choiceContainer.transform);
                btnObj.name = "RestoreArtifactButton";
                var txt = btnObj.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = "place from bag";
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

        private void Update()
        {
            if (!_isOpen) return;

            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb.escapeKey.wasPressedThisFrame)
                {
                    if (_restorePickerModal != null && _restorePickerModal.activeSelf)
                    {
                        CloseRestorePicker();
                    }
                    else
                    {
                        Close();
                    }
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
                        Close();
                    }
                }
            }
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

                _hiddenPlayerSr = _playerMovement.GetComponentInChildren<SpriteRenderer>();
                if (_hiddenPlayerSr != null && _hiddenPlayerSr.enabled)
                {
                    _hiddenPlayerSr.enabled = false;
                }
                else
                {
                    _hiddenPlayerSr = null;
                }
            }

            // Hide other gameplay HUD elements (Joystick, Hearts, Room Banner, Home Button)
            _hiddenHudObjects.Clear();
            var canvasTr = rootPanel != null ? rootPanel.transform.parent : transform;
            if (canvasTr != null)
            {
                foreach (Transform child in canvasTr)
                {
                    if (child.gameObject != rootPanel && child.gameObject.activeSelf)
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
                characterNameText.text = isGirl ? "Darla" : "Darrel";
                characterNameText.color = new Color(1f, 0.95f, 0.85f, 1f);
            }

            int rNum = artifact != null ? artifact.RoomNumber : 1;
            bool isPedestalRestored = artifact != null && artifact.IsRestored;
            bool allRead = GameProgression.AreAllPlaquesRead(rNum);
            int readCount = GameProgression.GetPlaquesReadCount(rNum);
            bool hasMatchingInBag = DoesPlayerHaveArtifactForPedestal(artifact);
            bool battleDone = GameProgression.HasRetrievedRoomArtifact(rNum);

            if (dialogueText != null)
            {
                if (isPedestalRestored)
                {
                    dialogueText.text = $"The {artifact.ArtifactName} has been safely restored to this pedestal!";
                }
                else if (hasMatchingInBag)
                {
                    dialogueText.text = $"I have the {artifact.ArtifactName} in my bag! I should place it on this pedestal.";
                }
                else if (battleDone)
                {
                    dialogueText.text = $"This pedestal is empty. The artifact was lost in the Alterworld battle and was not recovered.";
                }
                else if (allRead)
                {
                    if (rNum == 1)
                        dialogueText.text = "I have studied all 5 plaques in Room 1! The mysterious portal in the museum lobby is now awakened!";
                    else
                        dialogueText.text = $"I have studied all 5 plaques in Room {rNum}! The Alterworld trial for Room {rNum} is now unlocked!";
                }
                else
                {
                    dialogueText.text = $"I need to study all 5 historical plaques in this room ({readCount}/5 studied). What should I do?";
                }
                dialogueText.color = new Color(0.18f, 0.18f, 0.18f, 1f);
            }

            UpdateChoiceButtons();

            if (rootPanel != null)
                rootPanel.SetActive(true);
        }

        private void UpdateChoiceButtons()
        {
            if (choiceContainer != null) choiceContainer.SetActive(true);
            if (plaquePopup != null) plaquePopup.SetActive(false);
            if (artifactPopup != null) artifactPopup.SetActive(false);
            if (_restorePickerModal != null) _restorePickerModal.SetActive(false);

            EnsureRestoreButton();
            if (restoreArtifactButton != null)
            {
                bool isPedestalRestored = _currentArtifact != null && _currentArtifact.IsRestored;
                // Only show restore button if pedestal is NOT restored yet
                restoreArtifactButton.gameObject.SetActive(!isPedestalRestored);
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
            if (iname.Contains("manuscript") && pname.Contains("manuscript")) return true;
            if ((iname.Contains("quill") || iname.Contains("nib")) && (pname.Contains("quill") || pname.Contains("nib"))) return true;
            if ((iname.Contains("spectacles") || iname.Contains("glasses")) && (pname.Contains("spectacles") || pname.Contains("glasses"))) return true;
            if (iname.Contains("locket") && pname.Contains("locket")) return true;

            return false;
        }

        public void OnRestoreArtifactClicked()
        {
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (plaquePopup != null) plaquePopup.SetActive(false);
            if (artifactPopup != null) artifactPopup.SetActive(false);

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
            windowRt.sizeDelta = new Vector2(850f, 520f);
            var winImg = windowObj.GetComponent<Image>();
            winImg.color = new Color(0.97f, 0.94f, 0.88f, 1f);

            // Title
            var titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(windowObj.transform, false);
            var titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -25f);
            titleRt.sizeDelta = new Vector2(600f, 35f);
            var titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "SELECT ARTIFACT TO RESTORE";
            titleTmp.fontSize = 26f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.18f, 0.12f, 0.05f, 1f);

            // Subtitle
            var subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subObj.transform.SetParent(windowObj.transform, false);
            var subRt = subObj.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0.5f, 1f);
            subRt.anchorMax = new Vector2(0.5f, 1f);
            subRt.pivot = new Vector2(0.5f, 1f);
            subRt.anchoredPosition = new Vector2(0f, -60f);
            subRt.sizeDelta = new Vector2(700f, 25f);
            var subTmp = subObj.GetComponent<TextMeshProUGUI>();
            subTmp.text = "Pick an artifact from your bag to place on this pedestal:";
            subTmp.fontSize = 16f;
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.color = new Color(0.4f, 0.3f, 0.15f, 1f);

            // Grid Container
            var gridObj = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObj.transform.SetParent(windowObj.transform, false);
            var gridRt = gridObj.GetComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0.5f, 0.5f);
            gridRt.anchorMax = new Vector2(0.5f, 0.5f);
            gridRt.pivot = new Vector2(0.5f, 0.5f);
            gridRt.anchoredPosition = new Vector2(0f, -20f);
            gridRt.sizeDelta = new Vector2(760f, 340f);
            var glg = gridObj.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(360f, 95f);
            glg.spacing = new Vector2(20f, 15f);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;

            // Empty Notice
            var emptyNoticeObj = new GameObject("EmptyNotice", typeof(RectTransform), typeof(TextMeshProUGUI));
            emptyNoticeObj.transform.SetParent(windowObj.transform, false);
            var enRt = emptyNoticeObj.GetComponent<RectTransform>();
            enRt.anchorMin = new Vector2(0.5f, 0.5f);
            enRt.anchorMax = new Vector2(0.5f, 0.5f);
            enRt.pivot = new Vector2(0.5f, 0.5f);
            enRt.anchoredPosition = Vector2.zero;
            enRt.sizeDelta = new Vector2(650f, 80f);
            var enTmp = emptyNoticeObj.GetComponent<TextMeshProUGUI>();
            enTmp.text = "Your bag is empty.\nYou have no recovered artifacts to place.";
            enTmp.fontSize = 20f;
            enTmp.alignment = TextAlignmentOptions.Center;
            enTmp.color = new Color(0.5f, 0.35f, 0.2f, 0.85f);

            // Close Button
            var closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(windowObj.transform, false);
            var closeRt = closeBtnObj.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-20f, -20f);
            closeRt.sizeDelta = new Vector2(40f, 40f);
            var closeImg = closeBtnObj.GetComponent<Image>();
            closeImg.color = new Color(0.7f, 0.2f, 0.2f, 1f);
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
            closeTmp.fontSize = 22f;
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
                img.color = new Color(0.18f, 0.16f, 0.15f, 0.95f);

                var cardRt = cardObj.GetComponent<RectTransform>();
                cardRt.sizeDelta = new Vector2(360f, 95f);

                // Icon
                var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(cardObj.transform, false);
                var iconRt = iconObj.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0f, 0.5f);
                iconRt.anchorMax = new Vector2(0f, 0.5f);
                iconRt.pivot = new Vector2(0f, 0.5f);
                iconRt.anchoredPosition = new Vector2(14f, 0f);
                iconRt.sizeDelta = new Vector2(68f, 68f);
                var iconImg = iconObj.GetComponent<Image>();
                if (item.icon != null)
                {
                    iconImg.sprite = item.icon;
                    iconImg.color = Color.white;
                }
                else
                {
                    iconImg.color = new Color(1f, 1f, 1f, 0.2f);
                }
                iconImg.preserveAspect = true;

                // Name
                var nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
                nameObj.transform.SetParent(cardObj.transform, false);
                var nameRt = nameObj.GetComponent<RectTransform>();
                nameRt.anchorMin = new Vector2(0f, 1f);
                nameRt.anchorMax = new Vector2(1f, 1f);
                nameRt.pivot = new Vector2(0f, 1f);
                nameRt.anchoredPosition = new Vector2(95f, -10f);
                nameRt.sizeDelta = new Vector2(-195f, 26f);
                var nameTmp = nameObj.GetComponent<TextMeshProUGUI>();
                nameTmp.text = item.name;
                nameTmp.fontSize = 17f;
                nameTmp.fontStyle = FontStyles.Bold;
                nameTmp.color = new Color(0.96f, 0.85f, 0.55f, 1f);

                // Description
                var descObj = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
                descObj.transform.SetParent(cardObj.transform, false);
                var descRt = descObj.GetComponent<RectTransform>();
                descRt.anchorMin = new Vector2(0f, 0f);
                descRt.anchorMax = new Vector2(1f, 1f);
                descRt.pivot = new Vector2(0f, 0f);
                descRt.anchoredPosition = new Vector2(95f, 8f);
                descRt.sizeDelta = new Vector2(-195f, -38f);
                var descTmp = descObj.GetComponent<TextMeshProUGUI>();
                descTmp.text = item.description;
                descTmp.fontSize = 12f;
                descTmp.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);
                descTmp.overflowMode = TextOverflowModes.Ellipsis;

                // "PLACE" Button
                var placeBtnObj = new GameObject("PlaceButton", typeof(RectTransform), typeof(Image), typeof(Button));
                placeBtnObj.transform.SetParent(cardObj.transform, false);
                var placeRt = placeBtnObj.GetComponent<RectTransform>();
                placeRt.anchorMin = new Vector2(1f, 0.5f);
                placeRt.anchorMax = new Vector2(1f, 0.5f);
                placeRt.pivot = new Vector2(1f, 0.5f);
                placeRt.anchoredPosition = new Vector2(-10f, 0f);
                placeRt.sizeDelta = new Vector2(85f, 38f);
                var placeImg = placeBtnObj.GetComponent<Image>();
                placeImg.color = new Color(0.18f, 0.65f, 0.32f, 1f);

                var placeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                placeTxtObj.transform.SetParent(placeBtnObj.transform, false);
                var placeTxtRt = placeTxtObj.GetComponent<RectTransform>();
                placeTxtRt.anchorMin = Vector2.zero;
                placeTxtRt.anchorMax = Vector2.one;
                placeTxtRt.offsetMin = Vector2.zero;
                placeTxtRt.offsetMax = Vector2.zero;
                var placeTmp = placeTxtObj.GetComponent<TextMeshProUGUI>();
                placeTmp.text = "PLACE";
                placeTmp.fontSize = 15f;
                placeTmp.fontStyle = FontStyles.Bold;
                placeTmp.alignment = TextAlignmentOptions.Center;
                placeTmp.color = Color.white;

                var placeBtn = placeBtnObj.GetComponent<Button>();
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
                if (dialogueText != null)
                {
                    dialogueText.text = $"Hmm, '{item.name}' doesn't seem to belong on this pedestal. Let me check the other pedestals in this room.";
                }
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

            if (dialogueText != null)
            {
                dialogueText.text = $"I placed the {item.name} back on its pedestal where it belongs!";
            }

            // Check room restoration status
            int retrieved = GameProgression.GetRoomRetrievedCount(rNum);
            int restored = GameProgression.GetPedestalsRestoredCount(rNum);
            if (retrieved > 0 && restored >= retrieved)
            {
                NotificationBannerUI.Show(
                    $"ROOM {rNum} RESTORED!",
                    $"All recovered artifacts restored to Room {rNum}!\nReal World Room {rNum + 1} is now OPEN!",
                    6.0f,
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

            if (plaquePopup != null)
            {
                plaquePopup.SetActive(true);
                if (plaqueTitleText != null && _currentArtifact != null)
                    plaqueTitleText.text = _currentArtifact.PlaqueTitle;
                if (plaqueBodyText != null && _currentArtifact != null)
                    plaqueBodyText.text = _currentArtifact.PlaqueDescription;
            }

            if (dialogueText != null && _currentArtifact != null)
            {
                dialogueText.text = _currentArtifact.PlaqueDialogue;
            }

            // Record plaque read and check completion
            if (_currentArtifact != null)
            {
                int rNum = _currentArtifact.RoomNumber;
                string pId = !string.IsNullOrEmpty(_currentArtifact.ArtifactName) ? _currentArtifact.ArtifactName : _currentArtifact.gameObject.name;
                bool isNew = GameProgression.RecordPlaqueRead(rNum, pId);
                int totalRead = GameProgression.GetPlaquesReadCount(rNum);

                if (isNew)
                {
                    if (totalRead >= 5)
                    {
                        if (rNum == 1)
                        {
                            NotificationBannerUI.Show(
                                "PORTAL AWAKENED!",
                                "All 5 plaques in Room 1 studied!\nA mysterious portal has opened in the museum lobby!",
                                6.0f,
                                new Color(0.2f, 0.95f, 0.95f, 1f),
                                "★"
                            );
                        }
                        else
                        {
                            NotificationBannerUI.Show(
                                $"BATTLE {rNum} UNLOCKED!",
                                $"All 5 plaques in Room {rNum} studied!\nThe Alterworld trial for Room {rNum} is now accessible!",
                                6.0f,
                                new Color(0.2f, 0.95f, 0.4f, 1f),
                                "★"
                            );
                        }
                    }
                    else
                    {
                        NotificationBannerUI.Show(
                            "PLAQUE STUDIED",
                            $"Plaques studied: {totalRead}/5 in Room {rNum}",
                            2.5f,
                            new Color(1f, 0.85f, 0.35f, 1f),
                            "i"
                        );
                    }
                }
            }
        }

        public void OnInspectArtifactClicked()
        {
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (plaquePopup != null) plaquePopup.SetActive(false);
            if (_restorePickerModal != null) _restorePickerModal.SetActive(false);

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
                    artifactDetailTitleText.gameObject.SetActive(false);
                if (artifactDetailBodyText != null)
                    artifactDetailBodyText.gameObject.SetActive(false);
            }

            if (dialogueText != null)
            {
                if (isPedestalRestored)
                {
                    dialogueText.text = $"The sacred relic rests peacefully on its pedestal in Room {rNum}.";
                }
                else if (DoesPlayerHaveArtifactForPedestal(_currentArtifact))
                {
                    dialogueText.text = $"This pedestal is missing the {_currentArtifact.ArtifactName}. I have it in my bag! I can restore it now.";
                }
                else if (GameProgression.HasRetrievedRoomArtifact(rNum))
                {
                    dialogueText.text = $"The {_currentArtifact.ArtifactName} was lost in the Alterworld battle and was not recovered. It cannot be restored.";
                }
                else
                {
                    string diag = (_currentArtifact != null && !string.IsNullOrEmpty(_currentArtifact.ArtifactDialogue) && _currentArtifact.ArtifactDialogue != "what could this artifact be")
                        ? _currentArtifact.ArtifactDialogue
                        : "i wonder what this artifact looks like";
                    dialogueText.text = diag;
                }
            }
        }

        public void Close()
        {
            _isOpen = false;
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

            // Restore player sprite
            if (_hiddenPlayerSr != null)
            {
                _hiddenPlayerSr.enabled = true;
                _hiddenPlayerSr = null;
            }

            // Re-enable player movement
            if (_playerMovement != null)
                _playerMovement.enabled = true;

            _currentArtifact = null;
        }
    }
}
