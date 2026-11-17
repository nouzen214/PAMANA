using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TopDownGame
{
    /// <summary>
    /// Persistent singleton overlay providing smooth full-screen fade transitions between scenes or realms.
    /// </summary>
    public class ScreenFader : MonoBehaviour
    {
        private static ScreenFader _instance;
        public static ScreenFader Instance
        {
            get
            {
                if (_instance == null)
                {
                    var existing = FindFirstObjectByType<ScreenFader>();
                    if (existing != null)
                    {
                        _instance = existing;
                    }
                    else
                    {
                        var go = new GameObject("ScreenFader");
                        _instance = go.AddComponent<ScreenFader>();
                        if (Application.isPlaying) DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        public static bool IsFadedOut => _instance != null && _instance.Alpha > 0.5f;

        private Canvas _canvas;
        private Image _fadeImage;
        private Coroutine _fadeCoroutine;

        public bool IsFading { get; private set; }
        public float Alpha => _fadeImage != null ? _fadeImage.color.a : 0f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            EnsureUI();
        }

        private void EnsureUI()
        {
            if (_fadeImage != null) return;

            var canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform, false);

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9998; // Immediately below topmost alert/save overlays

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            var imgObj = new GameObject("FadeImage");
            imgObj.transform.SetParent(canvasObj.transform, false);

            _fadeImage = imgObj.AddComponent<Image>();
            _fadeImage.color = new Color(0f, 0f, 0f, 0f);
            _fadeImage.raycastTarget = false;

            var rt = imgObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// Smoothly fades the screen to the specified color (default: black).
        /// </summary>
        public void FadeOut(float duration, Color? color = null, Action onComplete = null)
        {
            EnsureUI();
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            Color targetCol = color ?? Color.black;
            _fadeCoroutine = StartCoroutine(FadeRoutine(1f, duration, targetCol, onComplete));
        }

        /// <summary>
        /// Smoothly fades the screen back to fully transparent.
        /// </summary>
        public void FadeIn(float duration, Action onComplete = null)
        {
            EnsureUI();
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            Color targetCol = _fadeImage != null ? _fadeImage.color : Color.black;
            _fadeCoroutine = StartCoroutine(FadeRoutine(0f, duration, targetCol, onComplete));
        }

        public IEnumerator FadeOutCoroutine(float duration, Color? color = null)
        {
            EnsureUI();
            bool done = false;
            FadeOut(duration, color, () => done = true);
            while (!done) yield return null;
        }

        public IEnumerator FadeInCoroutine(float duration)
        {
            EnsureUI();
            bool done = false;
            FadeIn(duration, () => done = true);
            while (!done) yield return null;
        }

        public void SetAlphaInstant(float alpha, Color? color = null)
        {
            EnsureUI();
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
            Color baseCol = color ?? (_fadeImage != null ? _fadeImage.color : Color.black);
            _fadeImage.color = new Color(baseCol.r, baseCol.g, baseCol.b, alpha);
            _fadeImage.raycastTarget = alpha > 0.5f;
            IsFading = false;
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, Color color, Action onComplete)
        {
            IsFading = true;
            _fadeImage.raycastTarget = targetAlpha > 0.05f;

            Color startCol = _fadeImage.color;
            Color endCol = new Color(color.r, color.g, color.b, targetAlpha);

            float startAlpha = startCol.a;
            float elapsed = 0f;

            if (duration <= 0.001f)
            {
                _fadeImage.color = endCol;
            }
            else
            {
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    // Smooth quadratic ease in-out
                    float smoothT = t * t * (3f - 2f * t);

                    _fadeImage.color = new Color(
                        Mathf.Lerp(startCol.r, endCol.r, smoothT),
                        Mathf.Lerp(startCol.g, endCol.g, smoothT),
                        Mathf.Lerp(startCol.b, endCol.b, smoothT),
                        Mathf.Lerp(startAlpha, targetAlpha, smoothT)
                    );
                    yield return null;
                }
                _fadeImage.color = endCol;
            }

            _fadeImage.raycastTarget = targetAlpha > 0.5f;
            IsFading = false;
            _fadeCoroutine = null;
            onComplete?.Invoke();
        }
    }
}

