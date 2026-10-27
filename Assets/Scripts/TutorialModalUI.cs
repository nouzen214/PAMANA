using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace TopDownGame
{
    /// <summary>
    /// Interactive tutorial modal that explains gameplay mechanics, controls,
    /// Room 1 investigation, portal mechanics, and Alterworld quiz battles.
    /// Automatically pops up on new game start in Lobby, and can be reopened
    /// anytime via the Book/Guide HUD button or pressing [H].
    /// </summary>
    public class TutorialModalUI : MonoBehaviour
    {
        public static TutorialModalUI Instance { get; private set; }

        [Header("Runtime State")]
        private GameObject _modalRoot;
        private CanvasGroup _canvasGroup;
        private int _currentPage = 0;
        private const int TOTAL_PAGES = 4;

        // UI References
        private TMP_Text _titleText;
        private TMP_Text _subtitleText;
        private TMP_Text _pageCounterText;
        private Button _prevButton;
        private Button _nextButton;
        private TMP_Text _nextButtonText;
        private Button _closeButton;

        private List<Button> _tabButtons = new List<Button>();
        private List<TMP_Text> _tabTexts = new List<TMP_Text>();
        private List<GameObject> _pageContainers = new List<GameObject>();
        private List<Image> _dotIndicators = new List<Image>();

        private readonly Color COLOR_GOLD = new Color(0.96f, 0.82f, 0.44f, 1f);
        private readonly Color COLOR_GOLD_MUTED = new Color(0.72f, 0.62f, 0.38f, 1f);
        private readonly Color COLOR_BG_DARK = new Color(0.09f, 0.08f, 0.12f, 0.98f);
        private readonly Color COLOR_CARD_BG = new Color(0.14f, 0.13f, 0.18f, 0.96f);
        private readonly Color COLOR_BORDER = new Color(0.85f, 0.72f, 0.42f, 0.85f);
        private readonly Color COLOR_KEYCAP_BG = new Color(0.24f, 0.22f, 0.30f, 1f);
        private readonly Color COLOR_TEXT_MUTED = new Color(0.78f, 0.78f, 0.82f, 1f);

        private float _previousTimeScale = 1f;

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
        }

        private void Start()
        {
            // Auto show tutorial on new game in Lobby
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName == "Lobby" && !GameProgression.HasCompletedTutorial)
            {
                Show(0);
            }
        }

        private void Update()
        {
            // Allow toggling tutorial anytime with H or F1 key
            if (Keyboard.current != null)
            {
                if (Keyboard.current.hKey.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame)
                {
                    Toggle();
                    return;
                }
            }

            if (_modalRoot == null || !_modalRoot.activeSelf) return;

            // Keyboard navigation while modal is open
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    Close();
                }
                else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
                {
                    NextPage();
                }
                else if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
                {
                    PreviousPage();
                }
                else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    if (_currentPage < TOTAL_PAGES - 1)
                        NextPage();
                    else
                        Close();
                }
            }
        }

        public static void Show(int startPage = 0)
        {
            var ui = Instance;
            if (ui == null)
            {
                ui = FindFirstObjectByType<TutorialModalUI>(FindObjectsInactive.Include);
            }

            if (ui == null)
            {
                ui = CreateInstance();
            }

            if (ui != null)
            {
                ui.OpenPage(startPage);
            }
        }

        public static void CloseModal()
        {
            if (Instance != null)
            {
                Instance.Close();
            }
        }

        public static void Toggle()
        {
            if (Instance != null && Instance._modalRoot != null && Instance._modalRoot.activeSelf)
            {
                Instance.Close();
            }
            else
            {
                Show(0);
            }
        }

        public void OpenPage(int pageIndex)
        {
            if (_modalRoot == null)
            {
                BuildUI();
            }

            _currentPage = Mathf.Clamp(pageIndex, 0, TOTAL_PAGES - 1);
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            _modalRoot.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            RefreshPageDisplay();
        }

        public void Close()
        {
            if (_modalRoot != null)
            {
                _modalRoot.SetActive(false);
            }

            Time.timeScale = _previousTimeScale > 0f ? _previousTimeScale : 1f;
            GameProgression.HasCompletedTutorial = true;
        }

        public void NextPage()
        {
            if (_currentPage < TOTAL_PAGES - 1)
            {
                _currentPage++;
                RefreshPageDisplay();
            }
            else
            {
                Close();
            }
        }

        public void PreviousPage()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                RefreshPageDisplay();
            }
        }

        private void RefreshPageDisplay()
        {
            // Update page containers
            for (int i = 0; i < _pageContainers.Count; i++)
            {
                if (_pageContainers[i] != null)
                {
                    _pageContainers[i].SetActive(i == _currentPage);
                }
            }

            // Update top tabs
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                bool isSelected = (i == _currentPage);
                var btnImg = _tabButtons[i].GetComponent<Image>();
                if (btnImg != null)
                {
                    btnImg.color = isSelected ? new Color(0.24f, 0.20f, 0.12f, 1f) : new Color(0.12f, 0.11f, 0.16f, 0.8f);
                }
                if (i < _tabTexts.Count && _tabTexts[i] != null)
                {
                    _tabTexts[i].color = isSelected ? COLOR_GOLD : new Color(0.65f, 0.65f, 0.70f, 1f);
                    _tabTexts[i].fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
                }
            }

            // Update dots
            for (int i = 0; i < _dotIndicators.Count; i++)
            {
                if (_dotIndicators[i] != null)
                {
                    _dotIndicators[i].color = (i == _currentPage) ? COLOR_GOLD : new Color(0.4f, 0.4f, 0.45f, 0.6f);
                    _dotIndicators[i].rectTransform.sizeDelta = (i == _currentPage) ? new Vector2(24f, 10f) : new Vector2(10f, 10f);
                }
            }

            // Navigation buttons
            if (_prevButton != null)
            {
                _prevButton.interactable = (_currentPage > 0);
                var prevText = _prevButton.GetComponentInChildren<TMP_Text>();
                if (prevText != null)
                {
                    prevText.color = (_currentPage > 0) ? Color.white : new Color(0.45f, 0.45f, 0.5f, 0.6f);
                }
            }

            if (_nextButtonText != null)
            {
                if (_currentPage == TOTAL_PAGES - 1)
                {
                    _nextButtonText.text = "START PLAYING! [OK]";
                    _nextButtonText.color = new Color(0.1f, 0.08f, 0.05f, 1f);
                    var img = _nextButton.GetComponent<Image>();
                    if (img != null) img.color = COLOR_GOLD;
                }
                else
                {
                    _nextButtonText.text = "NEXT PAGE ->";
                    _nextButtonText.color = Color.white;
                    var img = _nextButton.GetComponent<Image>();
                    if (img != null) img.color = new Color(0.25f, 0.22f, 0.32f, 1f);
                }
            }

            if (_pageCounterText != null)
            {
                _pageCounterText.text = $"PAGE {_currentPage + 1} OF {TOTAL_PAGES}";
            }

            // Headers based on current page
            switch (_currentPage)
            {
                case 0:
                    _titleText.text = "CONTROLS & EXPLORATION";
                    _subtitleText.text = "Learn how to walk, interact, and access your museum tools.";
                    break;
                case 1:
                    _titleText.text = "THE MYSTERY OF ROOM 1";
                    _subtitleText.text = "Investigate the crime scene where ancient Philippine relics were stolen.";
                    break;
                case 2:
                    _titleText.text = "THE ANCIENT PORTAL";
                    _subtitleText.text = "Awaken the supernatural rift in the Lobby to travel across dimensions.";
                    break;
                case 3:
                    _titleText.text = "QUIZ BATTLES & RECOVERY";
                    _subtitleText.text = "Challenge the shadow spirits with history trivia to reclaim the artifacts.";
                    break;
            }
        }

        private static TutorialModalUI CreateInstance()
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[TutorialModalUI] No Canvas found in scene!");
                return null;
            }

            var go = new GameObject("TutorialModalUI");
            go.transform.SetParent(canvas.transform, false);
            var tutorial = go.AddComponent<TutorialModalUI>();
            tutorial.BuildUI();
            return tutorial;
        }

        private void BuildUI()
        {
            if (_modalRoot != null) return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Fullscreen Dimmer Backdrop
            _modalRoot = new GameObject("TutorialModalBackdrop", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            _modalRoot.transform.SetParent(canvas.transform, false);
            _modalRoot.transform.SetAsLastSibling();

            var bgRt = _modalRoot.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var bgImg = _modalRoot.GetComponent<Image>();
            bgImg.color = new Color(0.04f, 0.04f, 0.07f, 0.88f);

            _canvasGroup = _modalRoot.GetComponent<CanvasGroup>();

            // 2. Centered Modal Window Frame
            var windowObj = new GameObject("TutorialWindow", typeof(RectTransform), typeof(Image));
            windowObj.transform.SetParent(_modalRoot.transform, false);

            var winRt = windowObj.GetComponent<RectTransform>();
            winRt.anchorMin = new Vector2(0.5f, 0.5f);
            winRt.anchorMax = new Vector2(0.5f, 0.5f);
            winRt.pivot = new Vector2(0.5f, 0.5f);
            winRt.sizeDelta = new Vector2(880f, 570f);

            var winImg = windowObj.GetComponent<Image>();
            winImg.color = COLOR_BG_DARK;

            // Gold Outer Accent Border
            var borderObj = new GameObject("GoldBorder", typeof(RectTransform), typeof(Image), typeof(Outline));
            borderObj.transform.SetParent(windowObj.transform, false);
            var borderRt = borderObj.GetComponent<RectTransform>();
            borderRt.anchorMin = Vector2.zero;
            borderRt.anchorMax = Vector2.one;
            borderRt.offsetMin = Vector2.zero;
            borderRt.offsetMax = Vector2.zero;
            var borderImg = borderObj.GetComponent<Image>();
            borderImg.color = new Color(0f, 0f, 0f, 0f);
            var outline = borderObj.GetComponent<Outline>();
            outline.effectColor = COLOR_BORDER;
            outline.effectDistance = new Vector2(2f, 2f);

            // 3. Header Bar
            var headerObj = new GameObject("HeaderBar", typeof(RectTransform), typeof(Image));
            headerObj.transform.SetParent(windowObj.transform, false);
            var hRt = headerObj.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f);
            hRt.anchorMax = new Vector2(1f, 1f);
            hRt.pivot = new Vector2(0.5f, 1f);
            hRt.anchoredPosition = Vector2.zero;
            hRt.sizeDelta = new Vector2(0f, 75f);
            var hImg = headerObj.GetComponent<Image>();
            hImg.color = new Color(0.14f, 0.12f, 0.18f, 1f);

            // Title & Subtitle Container
            var titleBox = new GameObject("TitleBox", typeof(RectTransform));
            titleBox.transform.SetParent(headerObj.transform, false);
            var tbRt = titleBox.GetComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0f, 0f);
            tbRt.anchorMax = new Vector2(1f, 1f);
            tbRt.offsetMin = new Vector2(25f, 5f);
            tbRt.offsetMax = new Vector2(-60f, -5f);

            var tTextObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            tTextObj.transform.SetParent(titleBox.transform, false);
            var ttRt = tTextObj.GetComponent<RectTransform>();
            ttRt.anchorMin = new Vector2(0f, 0.5f);
            ttRt.anchorMax = new Vector2(1f, 1f);
            ttRt.offsetMin = Vector2.zero;
            ttRt.offsetMax = Vector2.zero;
            _titleText = tTextObj.GetComponent<TextMeshProUGUI>();
            _titleText.text = "HOW TO PLAY";
            _titleText.fontSize = 22f;
            _titleText.fontStyle = FontStyles.Bold;
            _titleText.color = COLOR_GOLD;

            var subTextObj = new GameObject("SubtitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            subTextObj.transform.SetParent(titleBox.transform, false);
            var subRt = subTextObj.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0f, 0f);
            subRt.anchorMax = new Vector2(1f, 0.5f);
            subRt.offsetMin = Vector2.zero;
            subRt.offsetMax = Vector2.zero;
            _subtitleText = subTextObj.GetComponent<TextMeshProUGUI>();
            _subtitleText.text = "Essential gameplay mechanics";
            _subtitleText.fontSize = 13f;
            _subtitleText.color = COLOR_TEXT_MUTED;

            // Close Button [X]
            var closeObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeObj.transform.SetParent(headerObj.transform, false);
            var cRt = closeObj.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(1f, 0.5f);
            cRt.anchorMax = new Vector2(1f, 0.5f);
            cRt.pivot = new Vector2(1f, 0.5f);
            cRt.anchoredPosition = new Vector2(-15f, 0f);
            cRt.sizeDelta = new Vector2(36f, 36f);
            var cImg = closeObj.GetComponent<Image>();
            cImg.color = new Color(0.28f, 0.22f, 0.22f, 0.9f);
            _closeButton = closeObj.GetComponent<Button>();
            _closeButton.onClick.AddListener(Close);

            var closeLbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            closeLbl.transform.SetParent(closeObj.transform, false);
            var clRt = closeLbl.GetComponent<RectTransform>();
            clRt.anchorMin = Vector2.zero;
            clRt.anchorMax = Vector2.one;
            clRt.offsetMin = Vector2.zero;
            clRt.offsetMax = Vector2.zero;
            var cTmp = closeLbl.GetComponent<TextMeshProUGUI>();
            cTmp.text = "X";
            cTmp.fontSize = 18f;
            cTmp.fontStyle = FontStyles.Bold;
            cTmp.alignment = TextAlignmentOptions.Center;
            cTmp.color = Color.white;

            // 4. Tab Bar (Page switchers)
            var tabBarObj = new GameObject("TabBar", typeof(RectTransform));
            tabBarObj.transform.SetParent(windowObj.transform, false);
            var tabRt = tabBarObj.GetComponent<RectTransform>();
            tabRt.anchorMin = new Vector2(0f, 1f);
            tabRt.anchorMax = new Vector2(1f, 1f);
            tabRt.pivot = new Vector2(0.5f, 1f);
            tabRt.anchoredPosition = new Vector2(0f, -75f);
            tabRt.sizeDelta = new Vector2(0f, 40f);

            var tabLayout = tabBarObj.AddComponent<HorizontalLayoutGroup>();
            tabLayout.padding = new RectOffset(20, 20, 2, 2);
            tabLayout.spacing = 8f;
            tabLayout.childControlWidth = true;
            tabLayout.childControlHeight = true;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;

            string[] tabNames = new string[] { "1. CONTROLS", "2. ROOM 1 CLUES", "3. THE PORTAL", "4. QUIZ BATTLES" };
            _tabButtons.Clear();
            _tabTexts.Clear();
            for (int i = 0; i < tabNames.Length; i++)
            {
                int pageIdx = i;
                var tabBtnObj = new GameObject($"Tab_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                tabBtnObj.transform.SetParent(tabBarObj.transform, false);
                var btnImg = tabBtnObj.GetComponent<Image>();
                btnImg.color = new Color(0.12f, 0.11f, 0.16f, 0.8f);

                var tBtn = tabBtnObj.GetComponent<Button>();
                tBtn.onClick.AddListener(() =>
                {
                    _currentPage = pageIdx;
                    RefreshPageDisplay();
                });
                _tabButtons.Add(tBtn);

                var txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                txtObj.transform.SetParent(tabBtnObj.transform, false);
                var tRt2 = txtObj.GetComponent<RectTransform>();
                tRt2.anchorMin = Vector2.zero;
                tRt2.anchorMax = Vector2.one;
                tRt2.offsetMin = Vector2.zero;
                tRt2.offsetMax = Vector2.zero;
                var txt = txtObj.GetComponent<TextMeshProUGUI>();
                txt.text = tabNames[i];
                txt.fontSize = 13f;
                txt.alignment = TextAlignmentOptions.Center;
                txt.color = COLOR_TEXT_MUTED;
                _tabTexts.Add(txt);
            }

            // 5. Main Content Area (Multi-page switcher)
            var contentObj = new GameObject("PagesContentArea", typeof(RectTransform));
            contentObj.transform.SetParent(windowObj.transform, false);
            var contentRt = contentObj.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 0f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.offsetMin = new Vector2(25f, 65f);
            contentRt.offsetMax = new Vector2(-25f, -125f);

            _pageContainers.Clear();

            // Build Page 0: Controls
            _pageContainers.Add(BuildControlsPage(contentObj.transform));

            // Build Page 1: Room 1 Investigation
            _pageContainers.Add(BuildRoom1Page(contentObj.transform));

            // Build Page 2: The Portal
            _pageContainers.Add(BuildPortalPage(contentObj.transform));

            // Build Page 3: Quiz Battles
            _pageContainers.Add(BuildBattlesPage(contentObj.transform));

            // 6. Footer Navigation Bar
            var footerObj = new GameObject("FooterBar", typeof(RectTransform));
            footerObj.transform.SetParent(windowObj.transform, false);
            var fRt = footerObj.GetComponent<RectTransform>();
            fRt.anchorMin = new Vector2(0f, 0f);
            fRt.anchorMax = new Vector2(1f, 0f);
            fRt.pivot = new Vector2(0.5f, 0f);
            fRt.anchoredPosition = Vector2.zero;
            fRt.sizeDelta = new Vector2(0f, 65f);

            // Previous Button
            var prevObj = new GameObject("PrevButton", typeof(RectTransform), typeof(Image), typeof(Button));
            prevObj.transform.SetParent(footerObj.transform, false);
            var pRt = prevObj.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0f, 0.5f);
            pRt.anchorMax = new Vector2(0f, 0.5f);
            pRt.pivot = new Vector2(0f, 0.5f);
            pRt.anchoredPosition = new Vector2(25f, 0f);
            pRt.sizeDelta = new Vector2(130f, 42f);
            var pImg = prevObj.GetComponent<Image>();
            pImg.color = new Color(0.22f, 0.20f, 0.28f, 1f);
            _prevButton = prevObj.GetComponent<Button>();
            _prevButton.onClick.AddListener(PreviousPage);

            var prevLbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            prevLbl.transform.SetParent(prevObj.transform, false);
            var plRt = prevLbl.GetComponent<RectTransform>();
            plRt.anchorMin = Vector2.zero;
            plRt.anchorMax = Vector2.one;
            plRt.offsetMin = Vector2.zero;
            plRt.offsetMax = Vector2.zero;
            var plTmp = prevLbl.GetComponent<TextMeshProUGUI>();
            plTmp.text = "<- PREVIOUS";
            plTmp.fontSize = 13f;
            plTmp.fontStyle = FontStyles.Bold;
            plTmp.alignment = TextAlignmentOptions.Center;
            plTmp.color = Color.white;

            // Center Pagination Dots & Counter
            var centerBox = new GameObject("PaginationCenter", typeof(RectTransform));
            centerBox.transform.SetParent(footerObj.transform, false);
            var cBoxRt = centerBox.GetComponent<RectTransform>();
            cBoxRt.anchorMin = new Vector2(0.5f, 0.5f);
            cBoxRt.anchorMax = new Vector2(0.5f, 0.5f);
            cBoxRt.pivot = new Vector2(0.5f, 0.5f);
            cBoxRt.anchoredPosition = Vector2.zero;
            cBoxRt.sizeDelta = new Vector2(200f, 45f);

            var pageCounterObj = new GameObject("CounterText", typeof(RectTransform), typeof(TextMeshProUGUI));
            pageCounterObj.transform.SetParent(centerBox.transform, false);
            var pcRt = pageCounterObj.GetComponent<RectTransform>();
            pcRt.anchorMin = new Vector2(0f, 0.5f);
            pcRt.anchorMax = new Vector2(1f, 1f);
            pcRt.offsetMin = Vector2.zero;
            pcRt.offsetMax = Vector2.zero;
            _pageCounterText = pageCounterObj.GetComponent<TextMeshProUGUI>();
            _pageCounterText.text = "PAGE 1 OF 4";
            _pageCounterText.fontSize = 13f;
            _pageCounterText.fontStyle = FontStyles.Bold;
            _pageCounterText.alignment = TextAlignmentOptions.Center;
            _pageCounterText.color = COLOR_GOLD_MUTED;

            var dotsBox = new GameObject("DotsContainer", typeof(RectTransform));
            dotsBox.transform.SetParent(centerBox.transform, false);
            var dbRt = dotsBox.GetComponent<RectTransform>();
            dbRt.anchorMin = new Vector2(0f, 0f);
            dbRt.anchorMax = new Vector2(1f, 0.5f);
            dbRt.offsetMin = Vector2.zero;
            dbRt.offsetMax = Vector2.zero;
            var dLayout = dotsBox.AddComponent<HorizontalLayoutGroup>();
            dLayout.childAlignment = TextAnchor.MiddleCenter;
            dLayout.spacing = 8f;
            dLayout.childControlWidth = false;
            dLayout.childControlHeight = false;

            _dotIndicators.Clear();
            for (int i = 0; i < TOTAL_PAGES; i++)
            {
                var dotObj = new GameObject($"Dot_{i}", typeof(RectTransform), typeof(Image));
                dotObj.transform.SetParent(dotsBox.transform, false);
                var dImg = dotObj.GetComponent<Image>();
                dImg.color = (i == 0) ? COLOR_GOLD : new Color(0.4f, 0.4f, 0.45f, 0.6f);
                var dDotRt = dotObj.GetComponent<RectTransform>();
                dDotRt.sizeDelta = new Vector2(10f, 10f);
                _dotIndicators.Add(dImg);
            }

            // Next / Finish Button
            var nextObj = new GameObject("NextButton", typeof(RectTransform), typeof(Image), typeof(Button));
            nextObj.transform.SetParent(footerObj.transform, false);
            var nRt = nextObj.GetComponent<RectTransform>();
            nRt.anchorMin = new Vector2(1f, 0.5f);
            nRt.anchorMax = new Vector2(1f, 0.5f);
            nRt.pivot = new Vector2(1f, 0.5f);
            nRt.anchoredPosition = new Vector2(-25f, 0f);
            nRt.sizeDelta = new Vector2(185f, 42f);
            var nImg = nextObj.GetComponent<Image>();
            nImg.color = new Color(0.25f, 0.22f, 0.32f, 1f);
            _nextButton = nextObj.GetComponent<Button>();
            _nextButton.onClick.AddListener(NextPage);

            var nextLbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            nextLbl.transform.SetParent(nextObj.transform, false);
            var nlRt = nextLbl.GetComponent<RectTransform>();
            nlRt.anchorMin = Vector2.zero;
            nlRt.anchorMax = Vector2.one;
            nlRt.offsetMin = Vector2.zero;
            nlRt.offsetMax = Vector2.zero;
            _nextButtonText = nextLbl.GetComponent<TextMeshProUGUI>();
            _nextButtonText.text = "NEXT PAGE ->";
            _nextButtonText.fontSize = 13f;
            _nextButtonText.fontStyle = FontStyles.Bold;
            _nextButtonText.alignment = TextAlignmentOptions.Center;
            _nextButtonText.color = Color.white;

            _modalRoot.SetActive(false);
        }

        private GameObject BuildControlsPage(Transform parent)
        {
            var page = new GameObject("Page_Controls", typeof(RectTransform));
            page.transform.SetParent(parent, false);
            var rt = page.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var layout = page.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            CreateGuideCard(
                page.transform,
                "[W] [A] [S] [D]  /  [ARROW KEYS]",
                "WALKING & MOVEMENT",
                "Navigate through the historic museum galleries, hallways, and chambers. On touch or mobile devices, use the virtual analog joystick on the bottom-left.",
                new Color(0.20f, 0.35f, 0.45f, 1f)
            );

            CreateGuideCard(
                page.transform,
                "[E]  /  [F] KEY",
                "INTERACT & INSPECT",
                "Walk near doors, display pedestals, plaques, or glowing portals. When the interact prompt appears on your screen, press [E] to investigate or enter.",
                new Color(0.45f, 0.35f, 0.18f, 1f)
            );

            CreateGuideCard(
                page.transform,
                "[B] BAG  /  [H] GUIDE  /  [ESC] MENU",
                "HUD & INVENTORY",
                "Click the Backpack to view collected historical artifacts. Click the Field Guide (Book icon) to reopen this tutorial anytime, or Settings for volume options.",
                new Color(0.28f, 0.22f, 0.38f, 1f)
            );

            return page;
        }

        private GameObject BuildRoom1Page(Transform parent)
        {
            var page = new GameObject("Page_Room1", typeof(RectTransform));
            page.transform.SetParent(parent, false);
            var rt = page.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var layout = page.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            CreateGuideCard(
                page.transform,
                "STEP 1: MUSEUM LOCKDOWN",
                "HEAD TO THE LEFT WING",
                "The museum's ancient cultural relics were stolen overnight! Security has locked down other wings. Walk left into the Left Hallway to find Room 1 (Ancient Chamber).",
                new Color(0.35f, 0.18f, 0.18f, 1f)
            );

            CreateGuideCard(
                page.transform,
                "STEP 2: CRIME SCENE PEDESTALS",
                "OBSERVE THE BLACK SILHOUETTES",
                "Inside Room 1, you will find 3 pedestals showing solid black silhouettes (Ancient Seal of Tondo, Sacred Doctrina Manuscript, Illustrado Quill Nib). The black color indicates the artifact is missing!",
                new Color(0.18f, 0.28f, 0.38f, 1f)
            );

            CreateGuideCard(
                page.transform,
                "STEP 3: INVESTIGATION",
                "PRESS [E] TO INSPECT",
                "Walk right up to any of the 3 missing artifact pedestals and press [E]. Inspecting the scene uncovers vital clues and awakens the ancient portal in the Lobby!",
                new Color(0.42f, 0.32f, 0.15f, 1f)
            );

            return page;
        }

        private GameObject BuildPortalPage(Transform parent)
        {
            var page = new GameObject("Page_Portal", typeof(RectTransform));
            page.transform.SetParent(parent, false);
            var rt = page.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var layout = page.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            CreateGuideCard(
                page.transform,
                "EVENT: PORTAL AWAKENS",
                "ANCIENT RIFT OPENED",
                "Once you inspect an artifact in Room 1, the ancient portal in the central Museum Lobby activates! A notification banner will confirm the portal has opened.",
                new Color(0.18f, 0.36f, 0.42f, 1f)
            );

            CreateGuideCard(
                page.transform,
                "STEP: RETURN TO LOBBY",
                "BACKTRACK THROUGH THE HALLWAY",
                "Exit Room 1 through the bottom door, walk east through the hallway, and return to the main Museum Lobby.",
                new Color(0.24f, 0.22f, 0.35f, 1f)
            );

            CreateGuideCard(
                page.transform,
                "DESTINATION: THE ALTERWORLD",
                "STEP INTO THE PORTAL",
                "Approach the glowing blue vortex in the center of the Lobby. Step through to travel into the Alterworld where corrupted shadow monsters reside!",
                new Color(0.38f, 0.18f, 0.42f, 1f)
            );

            return page;
        }

        private GameObject BuildBattlesPage(Transform parent)
        {
            var page = new GameObject("Page_Battles", typeof(RectTransform));
            page.transform.SetParent(parent, false);
            var rt = page.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var layout = page.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            CreateGuideCard(
                page.transform,
                "BATTLE: SHADOW MONSTERS",
                "CORRUPTED GUARDIANS",
                "Corrupted shadow spirits hold the stolen relics. You must confront them in an intellectual quiz duel to purify and reclaim each artifact.",
                new Color(0.38f, 0.18f, 0.22f, 1f)
            );

            CreateGuideCard(
                page.transform,
                "ACTION: SELECT AN ARTIFACT SLOT",
                "CHOOSE YOUR CHALLENGE",
                "Click any of the 3 artifact slots at the bottom of the screen. Each slot triggers a trivia question regarding that specific cultural relic.",
                new Color(0.22f, 0.35f, 0.28f, 1f)
            );

            CreateGuideCard(
                page.transform,
                "VICTORY: ANSWER CORRECTLY",
                "RESTORE PHILIPPINE HERITAGE",
                "Select the correct answer from 4 choices to damage the monster and retrieve the artifact! Be careful: wrong answers cost hearts. Collect all 3 to triumph!",
                new Color(0.44f, 0.34f, 0.16f, 1f)
            );

            return page;
        }

        private void CreateGuideCard(Transform parent, string badgeText, string headingText, string bodyText, Color badgeColor)
        {
            var cardObj = new GameObject("Card", typeof(RectTransform), typeof(Image));
            cardObj.transform.SetParent(parent, false);

            var cImg = cardObj.GetComponent<Image>();
            cImg.color = COLOR_CARD_BG;

            var border = cardObj.AddComponent<Outline>();
            border.effectColor = new Color(0.28f, 0.26f, 0.34f, 0.8f);
            border.effectDistance = new Vector2(1f, 1f);

            var pillObj = new GameObject("Pill", typeof(RectTransform), typeof(Image));
            pillObj.transform.SetParent(cardObj.transform, false);
            var pRt = pillObj.GetComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0f, 0f);
            pRt.anchorMax = new Vector2(0f, 1f);
            pRt.pivot = new Vector2(0f, 0.5f);
            pRt.sizeDelta = new Vector2(5f, 0f);
            var pImg = pillObj.GetComponent<Image>();
            pImg.color = badgeColor;

            var headObj = new GameObject("CardHeader", typeof(RectTransform));
            headObj.transform.SetParent(cardObj.transform, false);
            var hRt = headObj.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f);
            hRt.anchorMax = new Vector2(1f, 1f);
            hRt.pivot = new Vector2(0f, 1f);
            hRt.anchoredPosition = new Vector2(18f, -10f);
            hRt.sizeDelta = new Vector2(-36f, 30f);

            var badgeObj = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            badgeObj.transform.SetParent(headObj.transform, false);
            var bRt = badgeObj.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0f, 0.5f);
            bRt.anchorMax = new Vector2(0f, 0.5f);
            bRt.pivot = new Vector2(0f, 0.5f);
            bRt.anchoredPosition = Vector2.zero;
            bRt.sizeDelta = new Vector2(280f, 26f);
            var bImg = badgeObj.GetComponent<Image>();
            bImg.color = badgeColor;

            var badgeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            badgeTxtObj.transform.SetParent(badgeObj.transform, false);
            var btRt = badgeTxtObj.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.offsetMin = new Vector2(8f, 0f);
            btRt.offsetMax = new Vector2(-8f, 0f);
            var btTmp = badgeTxtObj.GetComponent<TextMeshProUGUI>();
            btTmp.text = badgeText;
            btTmp.fontSize = 11.5f;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.alignment = TextAlignmentOptions.Center;
            btTmp.color = Color.white;

            var headTxtObj = new GameObject("Heading", typeof(RectTransform), typeof(TextMeshProUGUI));
            headTxtObj.transform.SetParent(headObj.transform, false);
            var htRt = headTxtObj.GetComponent<RectTransform>();
            htRt.anchorMin = new Vector2(0f, 0.5f);
            htRt.anchorMax = new Vector2(1f, 0.5f);
            htRt.pivot = new Vector2(0f, 0.5f);
            htRt.anchoredPosition = new Vector2(295f, 0f);
            htRt.sizeDelta = new Vector2(-300f, 26f);
            var htTmp = headTxtObj.GetComponent<TextMeshProUGUI>();
            htTmp.text = headingText;
            htTmp.fontSize = 14f;
            htTmp.fontStyle = FontStyles.Bold;
            htTmp.color = COLOR_GOLD;

            var bodyObj = new GameObject("BodyText", typeof(RectTransform), typeof(TextMeshProUGUI));
            bodyObj.transform.SetParent(cardObj.transform, false);
            var bdyRt = bodyObj.GetComponent<RectTransform>();
            bdyRt.anchorMin = new Vector2(0f, 0f);
            bdyRt.anchorMax = new Vector2(1f, 1f);
            bdyRt.offsetMin = new Vector2(18f, 10f);
            bdyRt.offsetMax = new Vector2(-18f, -44f);
            var bdyTmp = bodyObj.GetComponent<TextMeshProUGUI>();
            bdyTmp.text = bodyText;
            bdyTmp.fontSize = 13.5f;
            bdyTmp.lineSpacing = 1.2f;
            bdyTmp.color = new Color(0.88f, 0.88f, 0.92f, 1f);
        }
    }
}
