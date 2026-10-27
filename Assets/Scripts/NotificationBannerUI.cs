using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    /// <summary>
    /// Global notification banner for displaying game events, discoveries, and locked door alerts.
    /// Can be invoked statically via NotificationBannerUI.Show(...).
    /// </summary>
    public class NotificationBannerUI : MonoBehaviour
    {
        public static NotificationBannerUI Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private RectTransform bannerRect;
        [SerializeField] private CanvasGroup bannerGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image accentBorderImage;
        [SerializeField] private TMP_Text iconText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;

        [Header("Animation Settings")]
        [SerializeField] private float animDuration = 0.35f;

        private Coroutine _displayCoroutine;
        private Vector2 _targetPos = new Vector2(0f, -25f);
        private Vector2 _hiddenPos = new Vector2(0f, 100f);

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

        public static void Show(string title, string message, float duration = 4.5f, Color? accentColor = null, string icon = "!")
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
                banner.Display(title, message, duration, accentColor ?? new Color(0.2f, 0.9f, 0.9f, 1f), icon);
            }
            else
            {
                Debug.Log($"[NotificationBanner] {title}: {message}");
            }
        }

        public void Display(string title, string message, float duration, Color accentColor, string icon = "!")
        {
            if (titleText != null)
            {
                titleText.text = title.ToUpper();
                titleText.color = accentColor;
            }

            if (messageText != null)
            {
                messageText.text = message;
            }

            if (iconText != null)
            {
                iconText.text = icon;
            }

            if (accentBorderImage != null)
            {
                accentBorderImage.color = accentColor;
            }

            if (_displayCoroutine != null)
            {
                StopCoroutine(_displayCoroutine);
            }
            _displayCoroutine = StartCoroutine(ShowBannerRoutine(duration));
        }

        private IEnumerator ShowBannerRoutine(float duration)
        {
            gameObject.SetActive(true);
            if (bannerGroup != null) bannerGroup.blocksRaycasts = false;

            // Slide Down & Fade In
            float elapsed = 0f;
            while (elapsed < animDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animDuration);
                float easeOut = 1f - Mathf.Pow(1f - t, 3); // EaseOutCubic

                if (bannerGroup != null) bannerGroup.alpha = t;
                if (bannerRect != null) bannerRect.anchoredPosition = Vector2.Lerp(_hiddenPos, _targetPos, easeOut);

                yield return null;
            }

            if (bannerGroup != null) bannerGroup.alpha = 1f;
            if (bannerRect != null) bannerRect.anchoredPosition = _targetPos;

            // Hold on screen
            yield return new WaitForSeconds(duration);

            // Slide Up & Fade Out
            elapsed = 0f;
            while (elapsed < animDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animDuration);
                float easeIn = t * t;

                if (bannerGroup != null) bannerGroup.alpha = 1f - t;
                if (bannerRect != null) bannerRect.anchoredPosition = Vector2.Lerp(_targetPos, _hiddenPos, easeIn);

                yield return null;
            }

            if (bannerGroup != null) bannerGroup.alpha = 0f;
            if (bannerRect != null) bannerRect.anchoredPosition = _hiddenPos;
        }

        private static NotificationBannerUI CreateBannerInstance()
        {
            Canvas canvas = GameObject.Find("InGameCanvas")?.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null) return null;

            // Root banner container
            var rootObj = new GameObject("NotificationBanner", typeof(RectTransform), typeof(CanvasGroup));
            rootObj.transform.SetParent(canvas.transform, false);

            var rt = rootObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, 100f);
            rt.sizeDelta = new Vector2(660f, 96f);

            var cg = rootObj.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;

            // Background Card
            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(rootObj.transform, false);
            var bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0.08f, 0.07f, 0.11f, 0.96f);

            // Top Glowing Accent Line
            var borderObj = new GameObject("AccentLine", typeof(RectTransform), typeof(Image));
            borderObj.transform.SetParent(rootObj.transform, false);
            var bRt = borderObj.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0f, 1f);
            bRt.anchorMax = new Vector2(1f, 1f);
            bRt.pivot = new Vector2(0.5f, 1f);
            bRt.anchoredPosition = Vector2.zero;
            bRt.sizeDelta = new Vector2(0f, 4f);
            var bImg = borderObj.GetComponent<Image>();
            bImg.color = new Color(0.2f, 0.9f, 0.9f, 1f);

            // Icon Text (Left)
            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(TextMeshProUGUI));
            iconObj.transform.SetParent(rootObj.transform, false);
            var iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = new Vector2(45f, 0f);
            iconRt.sizeDelta = new Vector2(50f, 50f);
            var iconTmp = iconObj.GetComponent<TextMeshProUGUI>();
            iconTmp.text = "!";
            iconTmp.fontSize = 32f;
            iconTmp.alignment = TextAlignmentOptions.Center;

            // Title Text
            var titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(rootObj.transform, false);
            var tRt = titleObj.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 1f);
            tRt.anchorMax = new Vector2(1f, 1f);
            tRt.pivot = new Vector2(0f, 1f);
            tRt.anchoredPosition = new Vector2(85f, -14f);
            tRt.sizeDelta = new Vector2(-100f, 30f);
            var tTmp = titleObj.GetComponent<TextMeshProUGUI>();
            tTmp.text = "PORTAL OPENED";
            tTmp.fontSize = 22f;
            tTmp.fontStyle = FontStyles.Bold;
            tTmp.color = new Color(0.2f, 0.9f, 0.9f, 1f);

            // Message Text
            var msgObj = new GameObject("MessageText", typeof(RectTransform), typeof(TextMeshProUGUI));
            msgObj.transform.SetParent(rootObj.transform, false);
            var mRt = msgObj.GetComponent<RectTransform>();
            mRt.anchorMin = new Vector2(0f, 0f);
            mRt.anchorMax = new Vector2(1f, 0f);
            mRt.pivot = new Vector2(0f, 0f);
            mRt.anchoredPosition = new Vector2(85f, 14f);
            mRt.sizeDelta = new Vector2(-100f, 38f);
            var mTmp = msgObj.GetComponent<TextMeshProUGUI>();
            mTmp.text = "A mysterious portal has opened somewhere in the museum!";
            mTmp.fontSize = 17f;
            mTmp.color = new Color(0.95f, 0.95f, 0.95f, 1f);

            // Add component
            var comp = rootObj.AddComponent<NotificationBannerUI>();
            comp.bannerRect = rt;
            comp.bannerGroup = cg;
            comp.backgroundImage = bgImg;
            comp.accentBorderImage = bImg;
            comp.iconText = iconTmp;
            comp.titleText = tTmp;
            comp.messageText = mTmp;

            Instance = comp;
            return comp;
        }
    }
}
