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

            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
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
            var items = PlayerInventory.Items;

            if (countText != null)
            {
                countText.text = $"ARTIFACTS COLLECTED: {items.Count}";
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

        private GameObject CreateItemCard(ArtifactItemData item, Transform parent)
        {
            var cardObj = new GameObject($"Card_{item.id}", typeof(RectTransform), typeof(Image));
            cardObj.transform.SetParent(parent, false);

            var img = cardObj.GetComponent<Image>();
            img.color = new Color(0.18f, 0.16f, 0.15f, 0.95f);

            var cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(360f, 95f);

            // Icon Frame / Icon
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
                iconImg.color = new Color(1f, 1f, 1f, 0.15f);
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
            nameRt.sizeDelta = new Vector2(-105f, 28f);
            var nameTmp = nameObj.GetComponent<TextMeshProUGUI>();
            nameTmp.text = item.name;
            nameTmp.fontSize = 18f;
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
            descRt.sizeDelta = new Vector2(-105f, -40f);
            var descTmp = descObj.GetComponent<TextMeshProUGUI>();
            descTmp.text = item.description;
            descTmp.fontSize = 13f;
            descTmp.color = new Color(0.85f, 0.85f, 0.85f, 0.9f);
            descTmp.overflowMode = TextOverflowModes.Ellipsis;

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
            windowRt.sizeDelta = new Vector2(850f, 540f);
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
            titleRt.sizeDelta = new Vector2(500f, 40f);
            var titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "BAG / INVENTORY";
            titleTmp.fontSize = 28f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = new Color(0.18f, 0.12f, 0.05f, 1f);

            // Count
            var countObj = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
            countObj.transform.SetParent(windowObj.transform, false);
            var countRt = countObj.GetComponent<RectTransform>();
            countRt.anchorMin = new Vector2(0.5f, 1f);
            countRt.anchorMax = new Vector2(0.5f, 1f);
            countRt.pivot = new Vector2(0.5f, 1f);
            countRt.anchoredPosition = new Vector2(0f, -65f);
            countRt.sizeDelta = new Vector2(500f, 30f);
            var countTmp = countObj.GetComponent<TextMeshProUGUI>();
            countTmp.text = "ARTIFACTS COLLECTED: 0";
            countTmp.fontSize = 18f;
            countTmp.fontStyle = FontStyles.Bold;
            countTmp.alignment = TextAlignmentOptions.Center;
            countTmp.color = new Color(0.45f, 0.35f, 0.2f, 1f);

            // Grid Container
            var gridObj = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObj.transform.SetParent(windowObj.transform, false);
            var gridRt = gridObj.GetComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0.5f, 0.5f);
            gridRt.anchorMax = new Vector2(0.5f, 0.5f);
            gridRt.pivot = new Vector2(0.5f, 0.5f);
            gridRt.anchoredPosition = new Vector2(0f, -20f);
            gridRt.sizeDelta = new Vector2(760f, 360f);
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
            enRt.sizeDelta = new Vector2(600f, 100f);
            var enTmp = emptyNoticeObj.GetComponent<TextMeshProUGUI>();
            enTmp.text = "No artifacts in your bag yet.\nAnswer quizzes correctly to recover the artifacts!";
            enTmp.fontSize = 20f;
            enTmp.alignment = TextAlignmentOptions.Center;
            enTmp.color = new Color(0.45f, 0.35f, 0.2f, 0.8f);

            // Close Button
            var closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(windowObj.transform, false);
            var closeRt = closeBtnObj.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-20f, -20f);
            closeRt.sizeDelta = new Vector2(45f, 45f);
            var closeImg = closeBtnObj.GetComponent<Image>();
            closeImg.color = new Color(0.7f, 0.2f, 0.2f, 1f);
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
            closeTmp.fontSize = 22f;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.alignment = TextAlignmentOptions.Center;
            closeTmp.color = Color.white;

            var bagModal = modalObj.AddComponent<BagModalUI>();
            bagModal.modalRoot = modalObj;
            bagModal.itemsGridContainer = gridObj.transform;
            bagModal.titleText = titleTmp;
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
