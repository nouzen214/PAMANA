using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    /// <summary>
    /// Global hanging notification banner for displaying room progression, discoveries,
    /// battle alerts, and game events with a rustic parchment & chain sign style.
    /// Can be invoked statically via NotificationBannerUI.Show(...).
    /// </summary>
    public class NotificationBannerUI : MonoBehaviour
    {
        public static NotificationBannerUI Instance { get; private set; }

        private struct BannerRequest
        {
            public string title;
            public string message;
            public float duration;
            public Color accentColor;
            public string icon;
        }

        [Header("UI Elements")]
        [SerializeField] private RectTransform bannerRect;
        [SerializeField] private CanvasGroup bannerGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;

        [Header("Animation Settings")]
        [SerializeField] private float animDuration = 0.15f;

        private readonly Queue<BannerRequest> _requestQueue = new Queue<BannerRequest>();
        private Coroutine _queueCoroutine;
        private BannerRequest _lastRequest;
        private float _lastRequestTime = -99f;
        private bool _skipCurrentHold = false;

        private readonly Vector2 _targetPos = new Vector2(0f, 0f);
        private readonly Vector2 _hiddenPos = new Vector2(0f, 190f);

        private static readonly Color DefaultTitleBrown = new Color(0.24f, 0.14f, 0.07f, 1f); // #3E2412
        private static readonly Color DefaultBodyBrown = new Color(0.12f, 0.07f, 0.03f, 1f);  // #1F1208

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (bannerGroup != null)
            {
                bannerGroup.alpha = 0f;
                bannerGroup.blocksRaycasts = false;
            }

            if (bannerRect != null)
            {
                bannerRect.anchoredPosition = _hiddenPos;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static void Show(string title, string message, float duration = 2.0f, Color? accentColor = null, string icon = "!")
        {
            var banner = Instance;
            if (banner == null)
            {
                banner = FindFirstObjectByType<NotificationBannerUI>();
            }

            if (banner == null)
            {
                banner = CreateBannerInstance();
            }

            if (banner != null)
            {
                banner.EnqueueNotification(title, message, duration, accentColor ?? DefaultTitleBrown, icon);
            }
            else
            {
                Debug.Log($"[NotificationBanner] {title}: {message}");
            }
        }

        public void EnqueueNotification(string title, string message, float duration, Color accentColor, string icon = "!")
        {
            // Debounce identical notifications within 1.2 seconds to prevent rapid double-click stutter
            if (string.Equals(_lastRequest.title, title, System.StringComparison.OrdinalIgnoreCase) &&
                string.Equals(_lastRequest.message, message, System.StringComparison.OrdinalIgnoreCase) &&
                (Time.unscaledTime - _lastRequestTime) < 1.2f)
            {
                return;
            }

            // If the incoming notification is a counter/progression update (like "PLAQUE STUDIED"),
            // remove any pending older counter notifications in queue so we jump straight to the latest count!
            if (title.StartsWith("PLAQUE", System.StringComparison.OrdinalIgnoreCase))
            {
                var filtered = new List<BannerRequest>();
                while (_requestQueue.Count > 0)
                {
                    var item = _requestQueue.Dequeue();
                    if (!item.title.StartsWith("PLAQUE", System.StringComparison.OrdinalIgnoreCase))
                    {
                        filtered.Add(item);
                    }
                }
                foreach (var f in filtered) _requestQueue.Enqueue(f);
            }

            // Cap queue size to maximum 2 items so backlogs never build up
            while (_requestQueue.Count >= 2)
            {
                _requestQueue.Dequeue(); // drop oldest stale notification
            }

            // Responsive duration: scale down long durations so interactions feel fast and snappy
            float effectiveDuration = Mathf.Clamp(duration * 0.50f, 1.4f, 2.6f);

            var req = new BannerRequest
            {
                title = title,
                message = message,
                duration = effectiveDuration,
                accentColor = NormalizeTextColor(accentColor),
                icon = icon
            };

            _lastRequest = req;
            _lastRequestTime = Time.unscaledTime;

            _requestQueue.Enqueue(req);

            // Signal the currently running hold to immediately fast-forward so the new notification displays without lag
            _skipCurrentHold = true;

            if (_queueCoroutine == null)
            {
                _queueCoroutine = StartCoroutine(ProcessQueueRoutine());
            }
        }

        private static Color NormalizeTextColor(Color inColor)
        {
            // Ensure title text is dark enough to read clearly on the light parchment background
            float lum = (0.299f * inColor.r) + (0.587f * inColor.g) + (0.114f * inColor.b);
            if (lum > 0.45f)
            {
                // High brightness input (e.g. legacy cyan or yellow) -> adapt to warm rich brown/amber
                return new Color(0.26f, 0.15f, 0.08f, 1f);
            }
            return inColor;
        }

        private IEnumerator ProcessQueueRoutine()
        {
            gameObject.SetActive(true);
            if (bannerGroup != null) bannerGroup.blocksRaycasts = false;

            bool isFirst = true;

            while (_requestQueue.Count > 0)
            {
                var current = _requestQueue.Dequeue();

                if (isFirst)
                {
                    // Set content before initial slide down
                    if (titleText != null)
                    {
                        titleText.text = current.title.ToUpper();
                        titleText.color = current.accentColor;
                    }

                    if (messageText != null)
                    {
                        messageText.text = current.message;
                        messageText.color = DefaultBodyBrown;
                    }

                    // Fast Slide Down (0.15s EaseOutCubic)
                    float elapsed = 0f;
                    Vector2 startPos = bannerRect != null ? bannerRect.anchoredPosition : _hiddenPos;
                    while (elapsed < animDuration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        float t = Mathf.Clamp01(elapsed / animDuration);
                        float easeOut = 1f - Mathf.Pow(1f - t, 3);

                        if (bannerGroup != null) bannerGroup.alpha = t;
                        if (bannerRect != null) bannerRect.anchoredPosition = Vector2.Lerp(startPos, _targetPos, easeOut);

                        yield return null;
                    }
                    isFirst = false;
                }
                else
                {
                    // IN-PLACE FAST CROSS-FADE PUNCH (Only 0.12s total for rapid succession)
                    // Quick fade out text (0.06s)
                    float fadeOutElapsed = 0f;
                    while (fadeOutElapsed < 0.06f)
                    {
                        fadeOutElapsed += Time.unscaledDeltaTime;
                        float t = Mathf.Clamp01(fadeOutElapsed / 0.06f);
                        if (bannerGroup != null) bannerGroup.alpha = Mathf.Lerp(1f, 0.25f, t);
                        yield return null;
                    }

                    // Update content immediately
                    if (titleText != null)
                    {
                        titleText.text = current.title.ToUpper();
                        titleText.color = current.accentColor;
                    }

                    if (messageText != null)
                    {
                        messageText.text = current.message;
                        messageText.color = DefaultBodyBrown;
                    }

                    // Quick fade in text (0.06s)
                    float fadeInElapsed = 0f;
                    while (fadeInElapsed < 0.06f)
                    {
                        fadeInElapsed += Time.unscaledDeltaTime;
                        float t = Mathf.Clamp01(fadeInElapsed / 0.06f);
                        if (bannerGroup != null) bannerGroup.alpha = Mathf.Lerp(0.25f, 1f, t);
                        yield return null;
                    }
                }

                if (bannerGroup != null) bannerGroup.alpha = 1f;
                if (bannerRect != null) bannerRect.anchoredPosition = _targetPos;

                // Dynamic hold duration: if notifications are queued behind this, compress hold to 0.85s
                float holdTime = _requestQueue.Count > 0 ? 0.85f : current.duration;

                _skipCurrentHold = false;
                float holdElapsed = 0f;
                while (holdElapsed < holdTime)
                {
                    holdElapsed += Time.unscaledDeltaTime;
                    // If player triggered another interaction while this is showing,
                    // allow a brief moment (0.35s) so the user registers the current notification, then switch instantly!
                    if (_skipCurrentHold && holdElapsed >= 0.35f)
                    {
                        break;
                    }
                    yield return null;
                }
            }

            // Fast Slide Up & Hide (0.15s EaseInCubic)
            float outElapsed = 0f;
            Vector2 curPos = bannerRect != null ? bannerRect.anchoredPosition : _targetPos;
            while (outElapsed < animDuration)
            {
                outElapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(outElapsed / animDuration);
                float easeIn = t * t * t;

                if (bannerGroup != null) bannerGroup.alpha = 1f - t;
                if (bannerRect != null) bannerRect.anchoredPosition = Vector2.Lerp(curPos, _hiddenPos, easeIn);

                yield return null;
            }

            if (bannerGroup != null) bannerGroup.alpha = 0f;
            if (bannerRect != null) bannerRect.anchoredPosition = _hiddenPos;

            _queueCoroutine = null;
        }

        private static NotificationBannerUI CreateBannerInstance()
        {
            Canvas targetCanvas = null;

            var inGameGo = GameObject.Find("InGameCanvas");
            if (inGameGo != null && inGameGo.activeInHierarchy)
            {
                targetCanvas = inGameGo.GetComponent<Canvas>();
            }

            if (targetCanvas == null)
            {
                var battleGo = GameObject.Find("BattleCanvas");
                if (battleGo != null && battleGo.activeInHierarchy)
                {
                    targetCanvas = battleGo.GetComponent<Canvas>();
                }
            }

            if (targetCanvas == null)
            {
                var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (var c in canvases)
                {
                    if (c != null && c.gameObject.activeInHierarchy &&
                        (c.renderMode == RenderMode.ScreenSpaceOverlay || c.renderMode == RenderMode.ScreenSpaceCamera))
                    {
                        targetCanvas = c;
                        break;
                    }
                }
            }

            if (targetCanvas == null) return null;

            // Root banner container
            var rootObj = new GameObject("NotificationBanner", typeof(RectTransform), typeof(CanvasGroup));
            rootObj.transform.SetParent(targetCanvas.transform, false);
            rootObj.transform.SetAsLastSibling();

            // Override sorting so banner is ALWAYS rendered on top of dialogue, popups, and HUD
            var canvasComp = rootObj.AddComponent<Canvas>();
            canvasComp.overrideSorting = true;
            canvasComp.sortingOrder = 32000;
            rootObj.AddComponent<GraphicRaycaster>();

            var rt = rootObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, 190f);
            rt.sizeDelta = new Vector2(592f, 176f);

            var cg = rootObj.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;

            // Hanging Board Background (Sprite with chains & light parchment)
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(rootObj.transform, false);
            var bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;

            var frameSpr = Resources.Load<Sprite>("UI/NotificationBanner_Frame");
#if UNITY_EDITOR
            if (frameSpr == null)
            {
                frameSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/NotificationBanner_Frame.png");
            }
#endif
            if (frameSpr != null)
            {
                bgImg.sprite = frameSpr;
                bgImg.type = Image.Type.Simple;
                bgImg.preserveAspect = true;
            }
            else
            {
                bgImg.color = new Color(0.93f, 0.88f, 0.80f, 0.98f);
            }

            // Title Text (Upper part of parchment)
            var titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(rootObj.transform, false);
            var tRt = titleObj.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.5f, 1f);
            tRt.anchorMax = new Vector2(0.5f, 1f);
            tRt.pivot = new Vector2(0.5f, 0.5f);
            tRt.anchoredPosition = new Vector2(0f, -78f);
            tRt.sizeDelta = new Vector2(510f, 28f);

            var tTmp = titleObj.GetComponent<TextMeshProUGUI>();
            tTmp.text = "NOTIFICATION";
            tTmp.fontSize = 22f;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.alignment = TextAlignmentOptions.Center;
            tTmp.color = DefaultTitleBrown;
            tTmp.raycastTarget = false;

            // Message Text (Lower part of parchment)
            var msgObj = new GameObject("MessageText", typeof(RectTransform), typeof(TextMeshProUGUI));
            msgObj.transform.SetParent(rootObj.transform, false);
            var mRt = msgObj.GetComponent<RectTransform>();
            mRt.anchorMin = new Vector2(0.5f, 1f);
            mRt.anchorMax = new Vector2(0.5f, 1f);
            mRt.pivot = new Vector2(0.5f, 0.5f);
            mRt.anchoredPosition = new Vector2(0f, -116f);
            mRt.sizeDelta = new Vector2(520f, 48f);

            var mTmp = msgObj.GetComponent<TextMeshProUGUI>();
            mTmp.text = "Description message.";
            mTmp.fontSize = 17.5f;
            mTmp.fontStyle = FontStyles.Bold;
            mTmp.alignment = TextAlignmentOptions.Center;
            mTmp.textWrappingMode = TextWrappingModes.Normal;
            mTmp.color = DefaultBodyBrown;
            mTmp.raycastTarget = false;

            // Add NotificationBannerUI component
            var comp = rootObj.AddComponent<NotificationBannerUI>();
            comp.bannerRect = rt;
            comp.bannerGroup = cg;
            comp.backgroundImage = bgImg;
            comp.titleText = tTmp;
            comp.messageText = mTmp;

            Instance = comp;
            return comp;
        }
    }
}
