using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace TopDownGame
{
    [Serializable]
    public class BookTopicData
    {
        public string topicTag;
        public string topicTitle;

        [Header("Left Page")]
        public string leftSubtitle;
        [TextArea(4, 10)] public string leftBody;

        [Header("Right Page")]
        public string rightSubtitle;
        [TextArea(4, 10)] public string rightBody;
    }

    /// <summary>
    /// Open Book Modal UI that displays the Museum Field Guide book.
    /// Presents 12 comprehensive Philippine history topics formatted across
    /// a two-page spread (Left Page & Right Page per Topic).
    /// </summary>
    public class BookModalUI : MonoBehaviour
    {
        public static BookModalUI Instance { get; private set; }

        [Header("Modal Components")]
        [SerializeField] private GameObject modalRoot;
        [SerializeField] private RectTransform bookContainer;
        [SerializeField] private Image bookFrameImage;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button dimmerButton;
        [SerializeField] private TMP_Text topTopicBadgeText;

        [Header("Left Page Elements")]
        [SerializeField] private RectTransform leftPageContainer;
        [SerializeField] private TMP_Text leftHeaderText;
        [SerializeField] private TMP_Text leftBodyText;
        [SerializeField] private TMP_Text leftFooterText;

        [Header("Right Page Elements")]
        [SerializeField] private RectTransform rightPageContainer;
        [SerializeField] private TMP_Text rightHeaderText;
        [SerializeField] private TMP_Text rightBodyText;
        [SerializeField] private TMP_Text rightFooterText;

        [Header("Pagination State")]
        [SerializeField] private int currentPage = 1; // 1 to 12 topics
        [SerializeField] private int maxPages = 12;

        [Header("Book Topics Content")]
        [SerializeField] private List<BookTopicData> topics = new List<BookTopicData>();

        private PlayerMovement _playerMovement;
        private float _previousTimeScale = 1f;

        public bool IsOpen => modalRoot != null && modalRoot.activeSelf;
        public int CurrentPage => currentPage;
        public int MaxPages => maxPages;

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

            if (topics == null || topics.Count == 0)
            {
                topics = GetDefaultTopics();
            }
            maxPages = Mathf.Max(1, topics.Count);

            WireButtons();
            EnsureUIElements();

            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
        }

        private void WireButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseBook);
            }

            if (dimmerButton != null)
            {
                dimmerButton.onClick.RemoveAllListeners();
                dimmerButton.onClick.AddListener(CloseBook);
            }

            if (prevPageButton != null)
            {
                prevPageButton.onClick.RemoveAllListeners();
                prevPageButton.onClick.AddListener(OnPrevPageClicked);
            }

            if (nextPageButton != null)
            {
                nextPageButton.onClick.RemoveAllListeners();
                nextPageButton.onClick.AddListener(OnNextPageClicked);
            }
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                var kb = Keyboard.current;

                if (IsOpen)
                {
                    if (kb.escapeKey.wasPressedThisFrame)
                    {
                        CloseBook();
                        return;
                    }

                    if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
                    {
                        OnPrevPageClicked();
                    }
                    else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
                    {
                        OnNextPageClicked();
                    }
                }

                // Hotkey to toggle book when unlocked
                if (kb.hKey.wasPressedThisFrame || kb.f1Key.wasPressedThisFrame)
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

                    if (IsOpen)
                    {
                        CloseBook();
                    }
                    else
                    {
                        OpenBook();
                    }
                }
            }
        }

        public static void Show()
        {
            if (Instance == null)
            {
                Instance = FindFirstObjectByType<BookModalUI>(FindObjectsInactive.Include);
                if (Instance == null)
                {
                    var canvas = FindFirstObjectByType<Canvas>();
                    if (canvas != null)
                    {
                        Instance = CreateBookModal(canvas.transform);
                    }
                }
            }

            if (Instance != null)
            {
                Instance.OpenBook();
            }
        }

        public static void Hide()
        {
            if (Instance != null)
            {
                Instance.CloseBook();
            }
        }

        public void OpenBook()
        {
            // Close inventory bag if open
            var bag = FindFirstObjectByType<BagModalUI>(FindObjectsInactive.Include);
            if (bag != null && bag.IsOpen)
            {
                bag.CloseBag();
            }

            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
            }

            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            if (_playerMovement == null)
            {
                _playerMovement = FindFirstObjectByType<PlayerMovement>();
            }
            if (_playerMovement != null)
            {
                _playerMovement.enabled = false;
            }

            EnsureUIElements();
            UpdatePaginationUI();
        }

        public void CloseBook()
        {
            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }

            Time.timeScale = _previousTimeScale > 0f ? _previousTimeScale : 1f;

            if (_playerMovement != null)
            {
                _playerMovement.enabled = true;
            }
        }

        public void OnPrevPageClicked()
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdatePaginationUI();
            }
        }

        public void OnNextPageClicked()
        {
            if (currentPage < maxPages)
            {
                currentPage++;
                UpdatePaginationUI();
            }
        }

        public void UpdatePaginationUI()
        {
            if (topics == null || topics.Count == 0)
            {
                topics = GetDefaultTopics();
            }
            maxPages = Mathf.Max(1, topics.Count);
            currentPage = Mathf.Clamp(currentPage, 1, maxPages);

            int topicIdx = currentPage - 1;
            BookTopicData curTopic = (topicIdx >= 0 && topicIdx < topics.Count) ? topics[topicIdx] : null;

            int leftPageNum = currentPage * 2 - 1;
            int rightPageNum = currentPage * 2;

            if (curTopic != null)
            {
                // Left Page
                if (leftHeaderText != null)
                {
                    leftHeaderText.text = $"<align=center><color=#8B4513><size=13><b>— {curTopic.topicTag} —</b></size></color>\n" +
                                          $"<size=18.5><b><color=#3B1E08>{curTopic.topicTitle}</color></b></size>\n" +
                                          $"<size=14><color=#7D4E24><i>{curTopic.leftSubtitle}</i></color></size></align>";
                }
                if (leftBodyText != null)
                {
                    leftBodyText.text = curTopic.leftBody;
                }
                if (leftFooterText != null)
                {
                    leftFooterText.text = $"<align=center><size=14><b><color=#4A2810>— Page {leftPageNum} —</color></b></size></align>";
                }

                // Right Page
                if (rightHeaderText != null)
                {
                    rightHeaderText.text = $"<align=center><color=#8B4513><size=13><b>— {curTopic.topicTag} CONTINUED —</b></size></color>\n" +
                                           $"<size=17.5><b><color=#3B1E08>{curTopic.rightSubtitle}</color></b></size></align>";
                }
                if (rightBodyText != null)
                {
                    rightBodyText.text = curTopic.rightBody;
                }
                if (rightFooterText != null)
                {
                    rightFooterText.text = $"<align=center><size=14><b><color=#4A2810>— Page {rightPageNum} —</color></b></size></align>";
                }
            }

            // Top Badge
            if (topTopicBadgeText != null)
            {
                topTopicBadgeText.text = $"<size=15.5><b><color=#FFD700>TOPIC </color><color=#FFF4D0>{currentPage} / {maxPages}</color></b></size>";
            }

            // Navigation Buttons Visual State
            if (prevPageButton != null)
            {
                prevPageButton.gameObject.SetActive(true);
                prevPageButton.interactable = (currentPage > 1);
                var img = prevPageButton.GetComponent<Image>();
                if (img != null) img.color = (currentPage > 1) ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            }

            if (nextPageButton != null)
            {
                nextPageButton.gameObject.SetActive(true);
                nextPageButton.interactable = (currentPage < maxPages);
                var img = nextPageButton.GetComponent<Image>();
                if (img != null) img.color = (currentPage < maxPages) ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            }
        }

        private void EnsureUIElements()
        {
            if (leftPageContainer == null || rightPageContainer == null) return;

            // Ensure Left Page Text Elements
            if (leftHeaderText == null)
            {
                var tr = leftPageContainer.Find("Header");
                if (tr != null) leftHeaderText = tr.GetComponent<TMP_Text>();
                else leftHeaderText = CreateTMP("Header", leftPageContainer, new Vector2(0f, 0.76f), new Vector2(1f, 1f), 17f);
            }
            if (leftBodyText == null)
            {
                var tr = leftPageContainer.Find("Body");
                if (tr != null) leftBodyText = tr.GetComponent<TMP_Text>();
                else leftBodyText = CreateTMP("Body", leftPageContainer, new Vector2(0f, 0.08f), new Vector2(1f, 0.76f), 15.5f, TextAlignmentOptions.TopLeft, true);
            }
            if (leftFooterText == null)
            {
                var tr = leftPageContainer.Find("Footer");
                if (tr != null) leftFooterText = tr.GetComponent<TMP_Text>();
                else leftFooterText = CreateTMP("Footer", leftPageContainer, new Vector2(0f, 0f), new Vector2(1f, 0.08f), 14f, TextAlignmentOptions.Center);
            }

            // Ensure Right Page Text Elements
            if (rightHeaderText == null)
            {
                var tr = rightPageContainer.Find("Header");
                if (tr != null) rightHeaderText = tr.GetComponent<TMP_Text>();
                else rightHeaderText = CreateTMP("Header", rightPageContainer, new Vector2(0f, 0.79f), new Vector2(1f, 1f), 17f);
            }
            if (rightBodyText == null)
            {
                var tr = rightPageContainer.Find("Body");
                if (tr != null) rightBodyText = tr.GetComponent<TMP_Text>();
                else rightBodyText = CreateTMP("Body", rightPageContainer, new Vector2(0f, 0.08f), new Vector2(1f, 0.79f), 15.5f, TextAlignmentOptions.TopLeft, true);
            }
            if (rightFooterText == null)
            {
                var tr = rightPageContainer.Find("Footer");
                if (tr != null) rightFooterText = tr.GetComponent<TMP_Text>();
                else rightFooterText = CreateTMP("Footer", rightPageContainer, new Vector2(0f, 0f), new Vector2(1f, 0.08f), 14f, TextAlignmentOptions.Center);
            }

            // Ensure Top Badge with decorative background
            if (topTopicBadgeText == null && bookContainer != null)
            {
                var tr = bookContainer.Find("TopicBadge");
                if (tr != null) topTopicBadgeText = tr.GetComponentInChildren<TMP_Text>();
                else
                {
                    var badgeObj = new GameObject("TopicBadge", typeof(RectTransform), typeof(Image));
                    badgeObj.transform.SetParent(bookContainer, false);
                    var bRt = badgeObj.GetComponent<RectTransform>();
                    bRt.anchorMin = new Vector2(0.5f, 1f);
                    bRt.anchorMax = new Vector2(0.5f, 1f);
                    bRt.pivot = new Vector2(0.5f, 0f);
                    bRt.anchoredPosition = new Vector2(0f, 14f);
                    bRt.sizeDelta = new Vector2(280f, 38f);

                    var bImg = badgeObj.GetComponent<Image>();
                    bImg.color = new Color(0.16f, 0.09f, 0.05f, 0.94f); // Deep wood-brown pill banner

                    topTopicBadgeText = CreateTMP("Text", badgeObj.transform, Vector2.zero, Vector2.one, 15f, TextAlignmentOptions.Center);
                }
            }
        }

        private static TextMeshProUGUI CreateTMP(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, float fontSize, TextAlignmentOptions align = TextAlignmentOptions.Center, bool autoSize = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = new Color(0.18f, 0.12f, 0.08f, 1f); // Rich dark sepia ink
            tmp.richText = true;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.lineSpacing = 10f;
            if (autoSize)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 13f;
                tmp.fontSizeMax = fontSize;
            }
            return tmp;
        }

        public static BookModalUI CreateBookModal(Transform parent)
        {
            var modalObj = new GameObject("BookModal", typeof(RectTransform));
            modalObj.transform.SetParent(parent, false);

            var rootRt = modalObj.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            var modalComp = modalObj.AddComponent<BookModalUI>();

            // Dimmer
            var dimmerObj = new GameObject("Dimmer", typeof(RectTransform), typeof(Image), typeof(Button));
            dimmerObj.transform.SetParent(modalObj.transform, false);
            var dimmerRt = dimmerObj.GetComponent<RectTransform>();
            dimmerRt.anchorMin = Vector2.zero;
            dimmerRt.anchorMax = Vector2.one;
            dimmerRt.offsetMin = Vector2.zero;
            dimmerRt.offsetMax = Vector2.zero;

            var dimmerImg = dimmerObj.GetComponent<Image>();
            dimmerImg.color = new Color(0f, 0f, 0f, 0.78f);
            dimmerImg.raycastTarget = true;

            var dimmerBtn = dimmerObj.GetComponent<Button>();
            dimmerBtn.transition = Selectable.Transition.None;

            // Book Container (size 1140 x 953 maintaining 670:560 ratio)
            var bookContObj = new GameObject("BookContainer", typeof(RectTransform));
            bookContObj.transform.SetParent(modalObj.transform, false);
            var bookContRt = bookContObj.GetComponent<RectTransform>();
            bookContRt.anchorMin = new Vector2(0.5f, 0.5f);
            bookContRt.anchorMax = new Vector2(0.5f, 0.5f);
            bookContRt.pivot = new Vector2(0.5f, 0.5f);
            bookContRt.anchoredPosition = Vector2.zero;
            bookContRt.sizeDelta = new Vector2(1140f, 953f);

            // Open Book Frame
            var bookFrameObj = new GameObject("BookFrame", typeof(RectTransform), typeof(Image));
            bookFrameObj.transform.SetParent(bookContObj.transform, false);
            var bookFrameRt = bookFrameObj.GetComponent<RectTransform>();
            bookFrameRt.anchorMin = Vector2.zero;
            bookFrameRt.anchorMax = Vector2.one;
            bookFrameRt.offsetMin = Vector2.zero;
            bookFrameRt.offsetMax = Vector2.zero;

            var bookFrameImg = bookFrameObj.GetComponent<Image>();
            bookFrameImg.preserveAspect = true;
            bookFrameImg.raycastTarget = true;
#if UNITY_EDITOR
            var sprFrame = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Book/Book_Open_Frame.png");
            if (sprFrame != null) bookFrameImg.sprite = sprFrame;
#endif

            // Close Button [X] (top right corner of book)
            var closeObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeObj.transform.SetParent(bookContObj.transform, false);
            var closeRt = closeObj.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-16f, -16f);
            closeRt.sizeDelta = new Vector2(46f, 46f);

            var closeImg = closeObj.GetComponent<Image>();
#if UNITY_EDITOR
            var sprClose = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Settings/Settings_Btn_Close_Clean.png");
            if (sprClose != null) closeImg.sprite = sprClose;
#endif
            closeImg.preserveAspect = true;
            closeImg.raycastTarget = true;

            var closeBtn = closeObj.GetComponent<Button>();

            // Left Arrow Button (Prev Page)
            var prevObj = new GameObject("Button_PrevPage", typeof(RectTransform), typeof(Image), typeof(Button));
            prevObj.transform.SetParent(bookContObj.transform, false);
            var prevRt = prevObj.GetComponent<RectTransform>();
            prevRt.anchorMin = new Vector2(0.5f, 0.5f);
            prevRt.anchorMax = new Vector2(0.5f, 0.5f);
            prevRt.pivot = new Vector2(0.5f, 0.5f);
            prevRt.anchoredPosition = new Vector2(-650f, -10f);
            prevRt.sizeDelta = new Vector2(104f, 130f);

            var prevImg = prevObj.GetComponent<Image>();
            prevImg.preserveAspect = true;
            prevImg.raycastTarget = true;
#if UNITY_EDITOR
            var sprLeft = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Book/Book_Arrow_Left.png");
            if (sprLeft != null) prevImg.sprite = sprLeft;
#endif

            var prevBtn = prevObj.GetComponent<Button>();
            var pColors = prevBtn.colors;
            pColors.normalColor = Color.white;
            pColors.highlightedColor = new Color(1f, 0.95f, 0.8f, 1f);
            pColors.pressedColor = new Color(0.85f, 0.75f, 0.6f, 1f);
            pColors.disabledColor = new Color(1f, 1f, 1f, 0.35f);
            prevBtn.colors = pColors;

            // Right Arrow Button (Next Page)
            var nextObj = new GameObject("Button_NextPage", typeof(RectTransform), typeof(Image), typeof(Button));
            nextObj.transform.SetParent(bookContObj.transform, false);
            var nextRt = nextObj.GetComponent<RectTransform>();
            nextRt.anchorMin = new Vector2(0.5f, 0.5f);
            nextRt.anchorMax = new Vector2(0.5f, 0.5f);
            nextRt.pivot = new Vector2(0.5f, 0.5f);
            nextRt.anchoredPosition = new Vector2(650f, -10f);
            nextRt.sizeDelta = new Vector2(104f, 130f);

            var nextImg = nextObj.GetComponent<Image>();
            nextImg.preserveAspect = true;
            nextImg.raycastTarget = true;
#if UNITY_EDITOR
            var sprRight = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Book/Book_Arrow_Right.png");
            if (sprRight != null) nextImg.sprite = sprRight;
#endif

            var nextBtn = nextObj.GetComponent<Button>();
            nextBtn.colors = pColors;

            // Left Page Container (positioned comfortably inside clear parchment area)
            var leftContObj = new GameObject("LeftPageContainer", typeof(RectTransform));
            leftContObj.transform.SetParent(bookContObj.transform, false);
            var leftContRt = leftContObj.GetComponent<RectTransform>();
            leftContRt.anchorMin = new Vector2(0.10f, 0.24f);
            leftContRt.anchorMax = new Vector2(0.46f, 0.79f);
            leftContRt.offsetMin = Vector2.zero;
            leftContRt.offsetMax = Vector2.zero;

            // Right Page Container (positioned comfortably inside clear parchment area)
            var rightContObj = new GameObject("RightPageContainer", typeof(RectTransform));
            rightContObj.transform.SetParent(bookContObj.transform, false);
            var rightContRt = rightContObj.GetComponent<RectTransform>();
            rightContRt.anchorMin = new Vector2(0.54f, 0.24f);
            rightContRt.anchorMax = new Vector2(0.90f, 0.79f);
            rightContRt.offsetMin = Vector2.zero;
            rightContRt.offsetMax = Vector2.zero;

            // Wire serialized fields
            modalComp.modalRoot = modalObj;
            modalComp.bookContainer = bookContRt;
            modalComp.bookFrameImage = bookFrameImg;
            modalComp.prevPageButton = prevBtn;
            modalComp.nextPageButton = nextBtn;
            modalComp.closeButton = closeBtn;
            modalComp.dimmerButton = dimmerBtn;
            modalComp.leftPageContainer = leftContRt;
            modalComp.rightPageContainer = rightContRt;
            modalComp.topics = GetDefaultTopics();
            modalComp.maxPages = modalComp.topics.Count;

            closeBtn.onClick.AddListener(modalComp.CloseBook);
            dimmerBtn.onClick.AddListener(modalComp.CloseBook);
            prevBtn.onClick.AddListener(modalComp.OnPrevPageClicked);
            nextBtn.onClick.AddListener(modalComp.OnNextPageClicked);

            modalComp.EnsureUIElements();
            modalComp.UpdatePaginationUI();

            modalObj.SetActive(false);
            return modalComp;
        }

        public static List<BookTopicData> GetDefaultTopics()
        {
            return new List<BookTopicData>
            {
                // TOPIC 1
                new BookTopicData
                {
                    topicTag = "TOPIC 1",
                    topicTitle = "Early Life & Education of José Rizal",
                    leftSubtitle = "Calamba & Early Studies",
                    leftBody = "José Rizal was born on <b>June 19, 1861</b>, in Calamba, Laguna. His mother, <b>Teodora Alonso Realonda</b>, was one of his first teachers. She taught him how to read and pray at a young age.\n\nRizal later continued his education in Biñan, where <b>Maestro Justiniano Aquino Cruz</b> became his first formal school teacher.",
                    rightSubtitle = "Medical Training & European Travels",
                    rightBody = "Rizal later studied medicine because he wanted to help his mother with her failing eyesight. He pursued <b>ophthalmology</b>, the branch of medicine concerned with the eyes.\n\nHe continued his studies in Europe, including at the <b>Universidad Central de Madrid</b> and later trained in ophthalmology under <b>Dr. Otto Becker</b> in the University of Heidelberg in Germany.\n\nHis travels also allowed him to encounter different cultures, literature, and languages. Among these was <b>German</b>, which he learned during his time in Europe. His experiences abroad expanded his knowledge and influenced his later writings and ideas about the Philippines."
                },

                // TOPIC 2
                new BookTopicData
                {
                    topicTag = "TOPIC 2",
                    topicTitle = "Literary Masterpieces & Writing",
                    leftSubtitle = "Noli Me Tangere",
                    leftBody = "José Rizal used literature to express his observations about Philippine society during Spanish colonial rule. His two novels, <i>Noli Me Tangere</i> and <i>El Filibusterismo</i>, presented stories that showed the struggles and injustices experienced by Filipinos during that period.\n\nRizal’s first novel, <i>Noli Me Tangere</i>, was published in Berlin. Its publication was made possible through the financial support of <color=#1B6833><b>Maximo Viola</b></color>, who helped Rizal when he faced difficulties in printing the novel.\n\nThe story is set in the fictional town of San Diego, where characters such as <color=#1B6833><b>Sisa</b></color> represent the suffering experienced by ordinary Filipinos.",
                    rightSubtitle = "El Filibusterismo & Mi Último Adiós",
                    rightBody = "His second novel, <i>El Filibusterismo</i>, continued his criticism of colonial abuses and social injustice. It was published in <color=#1B6833><b>Ghent, Belgium</b></color>, with <color=#1B6833><b>Valentin Ventura</b></color> providing financial assistance that helped complete its printing.\n\nRizal also wrote <i>Mi Último Adiós</i>, a farewell poem expressing his love for his country. Before his execution, he secretly placed the manuscript inside an <color=#1B6833><b>alcohol cooking stove</b></color> and entrusted it to his sister Trinidad Rizal."
                },

                // TOPIC 3
                new BookTopicData
                {
                    topicTag = "TOPIC 3",
                    topicTitle = "Propaganda Movement & Nationalist Essays",
                    leftSubtitle = "Patriotic Press & Assimilation",
                    leftBody = "In 1882, Rizal’s first patriotic essay, <i>El Amor Patrio</i> (Love of Country), was published in <color=#1B6833><b>Diariong Tagalog</b></color>, a bilingual newspaper in Manila.\n\nLater, he became involved with <i>La Solidaridad</i>, a reformist newspaper that promoted <color=#1B6833><b>assimilation with Spain</b></color>. Filipino reformists wanted political and social reforms, including greater representation in the Spanish government.\n\nIn 1872, the tragic event of the <color=#1B6833><b>execution of GOMBURZA</b></color> awakened young Rizal’s sense of love for his country and strengthened his desire to understand the injustices faced by Filipinos under Spanish colonial rule.",
                    rightSubtitle = "Malolos Letter & Future Vision",
                    rightBody = "The Malolos Letter reflected ideas about education and the role of women in society. Written by Rizal in response to the young women of Malolos, the letter encouraged Filipino mothers to instill <color=#1B6833><b>love for the country</b></color> in their children.\n\nRizal also wrote <i>The Philippines a Century Hence</i>, a series of essays that examined the country’s past and possible future. The work considered how foreign powers, including the <color=#1B6833><b>United States</b></color>, might affect the Philippines."
                },

                // TOPIC 4
                new BookTopicData
                {
                    topicTag = "TOPIC 4",
                    topicTitle = "La Liga Filipina & Civic Reforms",
                    leftSubtitle = "Founding & Objectives",
                    leftBody = "On July 3, 1892, José Rizal founded <b>La Liga Filipina</b> in Tondo, Manila.\n\nIt was a civic organization that aimed to unite Filipinos and encourage cooperation among its members.\n\nOne of the primary objectives stated in its constitution was to provide <color=#1B6833><b>mutual protection among its members</b></color>.",
                    rightSubtitle = "Arrest, Exile & Successors",
                    rightBody = "However, Rizal’s involvement with La Liga was short-lived. Shortly after its founding, Spanish Governor General <color=#1B6833><b>Eulogio Despujol</b></color> ordered his arrest and deportation. Rizal was subsequently sent to Dapitan.\n\nAfter Rizal’s arrest, <color=#1B6833><b>Andres Bonifacio</b></color>, who had been a member of La Liga, later became one of the founders of the Katipunan.\n\nWhen La Liga was reorganized, <color=#1B6833><b>Apolinario Mabini</b></color> served as secretary of its Supreme Council. After the organization eventually dissolved, the <color=#1B6833><b>Cuerpo de Compromisarios</b></color> continued efforts to support <i>La Solidaridad</i>."
                },

                // TOPIC 5
                new BookTopicData
                {
                    topicTag = "TOPIC 5",
                    topicTitle = "Dapitan Exile & Community Leadership",
                    leftSubtitle = "Community Medicine & Education",
                    leftBody = "José Rizal spent part of his exile in Dapitan, where he became more than a physician. He used his knowledge and skills to serve the local community.\n\nAs a doctor, he successfully operated on his mother, <color=#1B6833><b>Teodora Alonso Realonda</b></color>, for cataracts.\n\nRizal also established a school in Talisay, where he personally taught around <color=#1B6833><b>sixteen young male</b></color> students without charging tuition.",
                    rightSubtitle = "Innovations, Science & Medical Mission",
                    rightBody = "He had also worked on projects that improved the community. He introduced <color=#1B6833><b>coconut oil streetlamps</b></color> to illuminate the streets of Dapitan.\n\nHis interests extended to science and agriculture. A horned beetle discovered by Rizal in Dapitan was scientifically named <color=#1B6833><b>Apogonia rizali</b></color> in his honor.\n\nIn 1896, Dr. Pio Valenzuela, a Katipunan emissary, secretly visited Rizal in Dapitan. Rizal later requested permission to serve as a military surgeon in a <color=#1B6833><b>Spanish army hospital in Cuba</b></color>. He hoped to use his medical skills to provide humanitarian medical assistance to wounded soldiers and patients."
                },

                // TOPIC 6
                new BookTopicData
                {
                    topicTag = "TOPIC 6",
                    topicTitle = "Spanish Colonial Administration & Institutions",
                    leftSubtitle = "Central Government & Judicial Court",
                    leftBody = "During the Spanish colonial period, the Philippines was governed under a centralized colonial system.\n\nThe <color=#1B6833><b>Governor-General</b></color> was the highest colonial official and represented the Spanish Monarch in the Philippines.\n\nThe <color=#1B6833><b>Real Audiencia</b></color> served as the highest judicial court and also advised the Governor-General.",
                    rightSubtitle = "Frailocracia & Economic Extraction",
                    rightBody = "Spanish religious orders also had considerable influence over colonial affairs. The term <color=#1B6833><b>frailocracia</b></color> was used to describe the political and social dominance of Spanish friars during the colonial period.\n\nThe colonial government also used economic systems to collect resources and support its administration. Under the <color=#1B6833><b>bandala system</b></color>, farmers were required to sell agricultural products to the government at fixed prices.\n\nThe government also received financial support through the <color=#1B6833><b>Real Situado</b></color>, an annual financial grant from New Spain, or present-day Mexico."
                },

                // TOPIC 7
                new BookTopicData
                {
                    topicTag = "TOPIC 7",
                    topicTitle = "The Propaganda Movement & La Solidaridad",
                    leftSubtitle = "Reformist Press & La Solidaridad",
                    leftBody = "The Propaganda Movement was a reform movement led by Filipino intellectuals and reformists, many of whom worked from Spain. They used writing to call for reforms in the Philippines.\n\nOne of their major publications was <i>La Solidaridad</i>, first published in Barcelona in 1889 with <color=#1B6833><b>Graciano López Jaena</b></color> as its founding editor-in-chief.",
                    rightSubtitle = "Satire, Arts & Madrid Headquarters",
                    rightBody = "The movement also used literature and satire to criticize abuses during the Spanish colonial period. López Jaena wrote <color=#1B6833><b>Fray Botod</b></color>, which criticizes the abuse of Spanish friars. In 1884, <color=#1B6833><b>Juan Luna</b></color> won a first-class gold medal at the Madrid Fine Arts Exposition for his painting <i>Spolarium</i>.\n\nIn late 1889, Marcelo H. del Pilar moved the headquarters of the reformists to <color=#1B6833><b>Madrid</b></color> to be closer to the Spanish Cortes, supported by the <color=#1B6833><b>Junta de Propaganda</b></color>. However, publication ceased in 1895 due to financial difficulties and the declining health of its leaders."
                },

                // TOPIC 8
                new BookTopicData
                {
                    topicTag = "TOPIC 8",
                    topicTitle = "The Katipunan & The Philippine Revolution",
                    leftSubtitle = "Cry of Pugad Lawin & Initiation",
                    leftBody = "In August 1896, members of the Katipunan rejected Spanish authority during the Cry of Pugad Lawin by <color=#1B6833><b>tearing their cédulas</b></color>, symbolizing their rejection of colonial rule.\n\nNew members also underwent initiation rituals, including a <color=#1B6833><b>blood compact or blood-signing ceremony</b></color> to show their commitment to the organization.",
                    rightSubtitle = "Revolutionary Government & Battles",
                    rightBody = "The revolutionary movement later experienced political divisions. At the Tejeros Convention in March 1897, <color=#1B6833><b>Emilio Aguinaldo</b></color> was elected president of the revolutionary government.\n\nOn August 30, 1896, Katipunan forces fought the Spanish army in the <color=#1B6833><b>Battle of San Juan del Monte</b></color>.\n\nIn 1899, <color=#1B6833><b>Gregorio del Pillar</b></color> led Filipino forces in the Battle of Tirad Pass, where they held off American troops."
                },

                // TOPIC 9
                new BookTopicData
                {
                    topicTag = "TOPIC 9",
                    topicTitle = "Precolonial Culture, Artifacts & Traditions",
                    leftSubtitle = "Textiles & Pintados",
                    leftBody = "Long before Spanish colonization, communities throughout the Philippine Islands already had their own identity and cultures.\n\nThe <color=#1B6833><b>T’boli people</b></color> of South Cotabato are known for weaving <i>T’nalak</i>, a sacred textile made from abaca fibers.\n\nIn Visayan communities, the Spanish chroniclers referred to them as “Pintados” due to their <color=#1B6833><b>painted bodies</b></color>.",
                    rightSubtitle = "Writing, Carvings & Music",
                    rightBody = "Filipinos also developed writing systems before Spanish colonization. <color=#1B6833><b>Baybayin</b></color> was an indigenous syllabic script used commonly by the Tagalog-speaking groups.\n\nOther important traditions include the Ifugao <color=#1B6833><b>bulul</b></color>, a carved wooden figure associated with rice and protection of the harvest, and traditional <color=#1B6833><b>kulintang</b></color> music, which uses a set of tuned gongs."
                },

                // TOPIC 10
                new BookTopicData
                {
                    topicTag = "TOPIC 10",
                    topicTitle = "Philippine Mythology & Folklore",
                    leftSubtitle = "Deities & Celestial Lore",
                    leftBody = "Philippine mythology and folklore contain many stories about supernatural beings, deities, heroes, and creatures. These stories vary between different cultural communities and regions of the Philippines.\n\nIn Tagalog mythology, Bathala was regarded as a supreme creator deity. His daughter <color=#1B6833><b>Tala</b></color> was associated with the stars, while Mayari was associated with the moon. Stories about these deities helped explain natural phenomena and the world around people.",
                    rightSubtitle = "Legendary Creatures of Folklore",
                    rightBody = "Different regions also have their own legendary creatures:\n\nThe <color=#1B6833><b>Tikbalang</b></color> is commonly described as a tall humanoid creature with the head and legs of a horse.\n\nThe <color=#1B6833><b>Kapre</b></color> is traditionally described as a large tree-dwelling being often portrayed smoking a huge cigar.\n\nOther traditions include the <color=#1B6833><b>Bakunawa</b></color>, a gigantic serpent-like creature in Visayan mythology associated with eclipses, and the <color=#1B6833><b>Sarimanok</b></color>, a colorful legendary bird associated with the Maranao people."
                },

                // TOPIC 11
                new BookTopicData
                {
                    topicTag = "TOPIC 11",
                    topicTitle = "Philippine Historical Landmarks & Heritage",
                    leftSubtitle = "Indigenous Terraces & Burial Jars",
                    leftBody = "The Philippines has many historical places, objects, structures, and traditions that help preserve the country’s past. These forms of heritage show the creativity, beliefs, and experiences of Filipino communities across different periods.\n\nThe <color=#1B6833><b>Banaue Rice Terraces</b></color> represent the agricultural knowledge and efforts of Indigenous Cordillerian communities.\n\n<color=#1B6833><b>The Manunggul Jar</b></color>, discovered in Palawan, is a prehistoric burial jar known for the boat and two human figures depicted on its lid, representing a journey toward the afterlife.",
                    rightSubtitle = "Mindanao Pots & Colonial Sites",
                    rightBody = "In Mindanao, the <color=#1B6833><b>Maitum Anthropomorphic Pots</b></color> are ancient burial jars with lids shaped with human faces.\n\nSpanish colonial heritage can be seen in places such as <color=#1B6833><b>Paoay Church</b></color> in Ilocos Norte, known for its resistance against earthquakes.\n\nThe <color=#1B6833><b>Bamboo Organ</b></color> in St. Joseph Church in Las Piñas is a Philippine heritage site created by Father Diego Cera and uses bamboo pipes as part of its unique construction."
                },

                // TOPIC 12
                new BookTopicData
                {
                    topicTag = "TOPIC 12",
                    topicTitle = "Revolutionary Heroes & Key Historical Figures",
                    leftSubtitle = "Founders, Strategists & Generals",
                    leftBody = "The Philippine struggle for freedom involved many individuals who played different roles in the revolutionary movement and conflicts that followed.\n\n<color=#1B6833><b>Andres Bonifacio</b></color> was the founder and chief organizer of the Katipunan. He was known as the “Supremo” of the organization.\n\n<color=#1B6833><b>General Antonio Luna</b></color> served as a military strategist and commander of the Philippine Revolutionary Army.\n\n<color=#1B6833><b>Miguel Malvar</b></color> was known for his perseverance and became one of the last major Filipino generals to formally surrender in Luzon.",
                    rightSubtitle = "Heroines of the Revolution",
                    rightBody = "Women also played important roles.\n\n<color=#1B6833><b>Melchora Aquino</b></color>, known as Tandang Sora, provided food, shelter, and medical assistance to revolutionary fighters.\n\nIn the Visayas, <color=#1B6833><b>Teresa Magbanua</b></color>, a teacher from Iloilo, became a military leader and was later remembered as the “Visayan Joan of Arc”.\n\nThese individuals represent different forms of leadership from organizing secret societies and writing political ideas to providing medical support and leading soldiers in battle."
                }
            };
        }
    }
}
