using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    public class BagModalUI : MonoBehaviour
    {
        public static BagModalUI Instance { get; private set; }

        [Header("Modal Root")]
        [SerializeField] private GameObject modalRoot;
        [SerializeField] private Transform itemsGridContainer;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text emptyNoticeText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button dimmerButton;

        private readonly List<GameObject> _spawnedItemCards = new List<GameObject>();
        private PlayerMovement _playerMovement;

        public bool IsOpen => modalRoot != null && modalRoot.activeSelf;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseBag);
            }

            if (dimmerButton != null)
            {
                dimmerButton.onClick.RemoveAllListeners();
                dimmerButton.onClick.AddListener(CloseBag);
            }

            EnsureFrameStyle();
            ConfigureGridContainer();

            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
        }

        private void EnsureFrameStyle()
        {
            // Find window/dialog box
            Transform winTr = null;
            if (itemsGridContainer != null)
            {
                // itemsGridContainer could be inside Viewport or directly under Window
                winTr = itemsGridContainer.parent;
                if (winTr != null && winTr.name == "Viewport")
                {
                    var sv = winTr.parent;
                    if (sv != null) winTr = sv.parent;
                }
            }
            if (winTr == null && modalRoot != null)
            {
                winTr = modalRoot.transform.Find("Window");
                if (winTr == null) winTr = modalRoot.transform.Find("DialogBox");
            }

            if (winTr != null)
            {
                var winRt = winTr.GetComponent<RectTransform>();
                if (winRt != null)
                {
                    winRt.anchorMin = new Vector2(0.5f, 0.5f);
                    winRt.anchorMax = new Vector2(0.5f, 0.5f);
                    winRt.pivot = new Vector2(0.5f, 0.5f);
                    winRt.anchoredPosition = Vector2.zero;
                    winRt.sizeDelta = new Vector2(899f, 492f);
                }

                var winImg = winTr.GetComponent<Image>();
                if (winImg != null)
                {
                    var spr = Resources.Load<Sprite>("UI/BagInventory_Frame");
#if UNITY_EDITOR
                    if (spr == null)
                    {
                        spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/BagInventory_Frame.png");
                    }
#endif
                    if (spr != null)
                    {
                        winImg.sprite = spr;
                        winImg.type = Image.Type.Simple;
                        winImg.color = Color.white;
                    }
                }

                // If inside a ScrollView, adjust ScrollView / Grid positions to sit right in the slate area
                if (itemsGridContainer != null)
                {
                    Transform targetTr = itemsGridContainer;
                    if (targetTr.parent != null && targetTr.parent.name == "Viewport")
                    {
                        var sv = targetTr.parent.parent;
                        if (sv != null) targetTr = sv;
                    }

                    var tRt = targetTr.GetComponent<RectTransform>();
                    if (tRt != null)
                    {
                        tRt.anchorMin = new Vector2(0.5f, 0.5f);
                        tRt.anchorMax = new Vector2(0.5f, 0.5f);
                        tRt.pivot = new Vector2(0.5f, 0.5f);
                        tRt.anchoredPosition = new Vector2(0f, -36f);
                        tRt.sizeDelta = new Vector2(790f, 325f);
                    }
                }

                // Close button positioned on top-right brass corner
                if (closeButton != null)
                {
                    var cRt = closeButton.GetComponent<RectTransform>();
                    if (cRt != null)
                    {
                        cRt.anchorMin = new Vector2(1f, 1f);
                        cRt.anchorMax = new Vector2(1f, 1f);
                        cRt.pivot = new Vector2(1f, 1f);
                        cRt.anchoredPosition = new Vector2(-16f, -16f);
                        cRt.sizeDelta = new Vector2(34f, 34f);
                    }
                }

                // Redundant title is hidden (since "BAG / INVENTORY" is baked in flap art)
                if (titleText != null)
                {
                    titleText.gameObject.SetActive(false);
                }

                // Count text positioned right after "ARTIFACT COLLECTED:" on the flap
                if (countText != null)
                {
                    var cRt = countText.GetComponent<RectTransform>();
                    if (cRt != null)
                    {
                        cRt.anchorMin = new Vector2(0.5f, 0.5f);
                        cRt.anchorMax = new Vector2(0.5f, 0.5f);
                        cRt.pivot = new Vector2(0f, 0.5f);
                        cRt.anchoredPosition = new Vector2(82f, 169f);
                        cRt.sizeDelta = new Vector2(120f, 30f);
                    }
                    countText.alignment = TextAlignmentOptions.MidlineLeft;
                    countText.fontStyle = FontStyles.Bold;
                    countText.fontSize = 22f;
                    countText.color = new Color(0.24f, 0.16f, 0.08f, 1f);
                }

                // Empty notice text
                if (emptyNoticeText != null)
                {
                    var enRt = emptyNoticeText.GetComponent<RectTransform>();
                    if (enRt != null)
                    {
                        enRt.anchorMin = new Vector2(0.5f, 0.5f);
                        enRt.anchorMax = new Vector2(0.5f, 0.5f);
                        enRt.pivot = new Vector2(0.5f, 0.5f);
                        enRt.anchoredPosition = new Vector2(0f, -36f);
                        enRt.sizeDelta = new Vector2(650f, 100f);
                    }
                    emptyNoticeText.text = "No artifacts in your bag yet.\nAnswer quizzes correctly in Alterworld trials to recover artifacts!";
                    emptyNoticeText.fontSize = 20f;
                    emptyNoticeText.color = new Color(0.85f, 0.80f, 0.70f, 0.95f);
                }
            }
        }

        private void ConfigureGridContainer()
        {
            if (itemsGridContainer == null) return;

            var glg = itemsGridContainer.GetComponent<GridLayoutGroup>();
            if (glg == null) glg = itemsGridContainer.gameObject.AddComponent<GridLayoutGroup>();

            glg.cellSize = new Vector2(382f, 95f);
            glg.spacing = new Vector2(16f, 12f);
            glg.padding = new RectOffset(0, 0, 0, 0);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;
            glg.childAlignment = TextAnchor.UpperLeft;

            var csf = itemsGridContainer.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = itemsGridContainer.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void OnEnable()
        {
            PlayerInventory.OnInventoryChanged += RefreshUI;
        }

        private void OnDisable()
        {
            PlayerInventory.OnInventoryChanged -= RefreshUI;
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb.bKey.wasPressedThisFrame || kb.iKey.wasPressedThisFrame)
                {
                    if (IsOpen) CloseBag();
                    else OpenBag();
                }
                else if (IsOpen && kb.escapeKey.wasPressedThisFrame)
                {
                    CloseBag();
                }
            }
        }

        public void OpenBag()
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
            }

            if (_playerMovement == null)
            {
                _playerMovement = FindFirstObjectByType<PlayerMovement>();
            }
            if (_playerMovement != null)
            {
                _playerMovement.enabled = false;
            }

            RefreshUI();
        }

        public void CloseBag()
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }

            if (_playerMovement != null)
            {
                _playerMovement.enabled = true;
            }
        }

        public void RefreshUI()
        {
            EnsureFrameStyle();
            ConfigureGridContainer();

            var items = PlayerInventory.Items;

            if (countText != null)
            {
                countText.text = $"{items.Count}";
            }

            if (emptyNoticeText != null)
            {
                emptyNoticeText.gameObject.SetActive(items.Count == 0);
            }

            // Clear previously spawned cards
            foreach (var card in _spawnedItemCards)
            {
                if (card != null) Destroy(card);
            }
            _spawnedItemCards.Clear();

            if (itemsGridContainer == null) return;

            // Spawn a card for each collected artifact
            foreach (var item in items)
            {
                var card = CreateItemCard(item, itemsGridContainer);
                _spawnedItemCards.Add(card);
            }
        }

        private static Sprite ResolveArtifactSprite(ArtifactItemData item)
        {
            if (item == null) return null;
            if (item.icon != null) return item.icon;

            string[] searchKeys = new string[] { item.name, item.id };
            foreach (var k in searchKeys)
            {
                if (string.IsNullOrEmpty(k)) continue;
                string lower = k.ToLowerInvariant();
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

#if UNITY_EDITOR
                if (path != null)
                {
                    var s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (s != null) return s;
                }
#endif
            }
            return null;
        }

        private GameObject CreateItemCard(ArtifactItemData item, Transform parent)
        {
            var cardObj = new GameObject($"Card_{item.id}", typeof(RectTransform), typeof(Image));
            cardObj.transform.SetParent(parent, false);

            var img = cardObj.GetComponent<Image>();
            img.color = new Color(0.18f, 0.14f, 0.11f, 0.96f);

            var outline = cardObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.72f, 0.58f, 0.36f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(382f, 95f);

            // Icon Frame (light aged parchment square so artifact sprites stand out with high contrast)
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

            // Subtle inner backing to make bright/white items pop sharply
            var icBackObj = new GameObject("Backing", typeof(RectTransform), typeof(Image));
            icBackObj.transform.SetParent(iconFrameObj.transform, false);
            var icBackRt = icBackObj.GetComponent<RectTransform>();
            icBackRt.anchorMin = Vector2.zero;
            icBackRt.anchorMax = Vector2.one;
            icBackRt.offsetMin = new Vector2(3f, 3f);
            icBackRt.offsetMax = new Vector2(-3f, -3f);
            icBackObj.GetComponent<Image>().color = new Color(0.85f, 0.80f, 0.72f, 0.7f);

            // Artifact Icon (prominent, clear, 66x66)
            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(iconFrameObj.transform, false);
            var iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(5f, 5f);
            iconRt.offsetMax = new Vector2(-5f, -5f);

            var iconImg = iconObj.GetComponent<Image>();
            var spr = ResolveArtifactSprite(item);
            if (spr != null)
            {
                iconImg.sprite = spr;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = new Color(0.35f, 0.30f, 0.25f, 0.3f);
            }
            iconImg.preserveAspect = true;

            // Item Name (spacious, bold, large, centered vertically - no description clutter)
            var nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(cardObj.transform, false);
            var nameRt = nameObj.GetComponent<RectTransform>();
            nameRt.anchorMin = Vector2.zero;
            nameRt.anchorMax = Vector2.one;
            nameRt.offsetMin = new Vector2(100f, 6f);
            nameRt.offsetMax = new Vector2(-12f, -6f);

            var nameTmp = nameObj.GetComponent<TextMeshProUGUI>();
            nameTmp.text = item.name;
            nameTmp.fontSize = 21f;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.color = new Color(1f, 0.93f, 0.75f, 1f);
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            nameTmp.enableAutoSizing = true;
            nameTmp.fontSizeMin = 15f;
            nameTmp.fontSizeMax = 22f;
            nameTmp.textWrappingMode = TextWrappingModes.Normal;

            return cardObj;
        }

        /// <summary>
        /// Creates a BagModal hierarchy on demand if missing from a canvas.
        /// </summary>
        public static BagModalUI CreateBagModal(Transform canvasTransform)
        {
            var modalObj = new GameObject("BagModal", typeof(RectTransform));
            modalObj.transform.SetParent(canvasTransform, false);
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
            var sprBag = Resources.Load<Sprite>("UI/BagInventory_Frame");
#if UNITY_EDITOR
            if (sprBag == null) sprBag = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/BagInventory_Frame.png");
#endif
            if (sprBag != null) winImg.sprite = sprBag;

            // Count Text (positioned on flap next to "ARTIFACT COLLECTED:")
            var countObj = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
            countObj.transform.SetParent(windowObj.transform, false);
            var countRt = countObj.GetComponent<RectTransform>();
            countRt.anchorMin = new Vector2(0.5f, 0.5f);
            countRt.anchorMax = new Vector2(0.5f, 0.5f);
            countRt.pivot = new Vector2(0f, 0.5f);
            countRt.anchoredPosition = new Vector2(82f, 169f);
            countRt.sizeDelta = new Vector2(120f, 30f);
            var countTmp = countObj.GetComponent<TextMeshProUGUI>();
            countTmp.text = "0";
            countTmp.fontSize = 22f;
            countTmp.fontStyle = FontStyles.Bold;
            countTmp.alignment = TextAlignmentOptions.MidlineLeft;
            countTmp.color = new Color(0.24f, 0.16f, 0.08f, 1f);

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
            enRt.sizeDelta = new Vector2(650f, 100f);
            var enTmp = emptyNoticeObj.GetComponent<TextMeshProUGUI>();
            enTmp.text = "No artifacts in your bag yet.\nAnswer quizzes correctly in Alterworld trials to recover artifacts!";
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

            var bagModal = modalObj.AddComponent<BagModalUI>();
            bagModal.modalRoot = modalObj;
            bagModal.itemsGridContainer = gridObj.transform;
            bagModal.titleText = null;
            bagModal.countText = countTmp;
            bagModal.emptyNoticeText = enTmp;
            bagModal.closeButton = closeBtn;
            bagModal.dimmerButton = dimmerBtn;

            closeBtn.onClick.AddListener(bagModal.CloseBag);
            dimmerBtn.onClick.AddListener(bagModal.CloseBag);

            modalObj.SetActive(false);
            return bagModal;
        }
    }
}
