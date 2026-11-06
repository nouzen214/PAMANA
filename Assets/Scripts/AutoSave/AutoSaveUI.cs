using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TopDownGame
{
    public class AutoSaveUI : MonoBehaviour
    {
        private static AutoSaveUI _instance;
        public static AutoSaveUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AutoSaveUI");
                    _instance = go.AddComponent<AutoSaveUI>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private CanvasGroup _canvasGroup;
        private RectTransform _badgeRt;
        private RectTransform _iconRt;
        private TextMeshProUGUI _labelTmp;
        private Image _iconImg;

        private Coroutine _displayCoroutine;
        private bool _isSaving;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            BuildUI();
        }

        private void BuildUI()
        {
            var canvasObj = new GameObject("AutoSaveOverlayCanvas");
            canvasObj.transform.SetParent(transform);

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Topmost overlay

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Badge container (bottom right corner)
            var badgeObj = new GameObject("AutoSaveBadge");
            badgeObj.transform.SetParent(canvasObj.transform, false);

            _badgeRt = badgeObj.AddComponent<RectTransform>();
            _badgeRt.anchorMin = new Vector2(1f, 0f);
            _badgeRt.anchorMax = new Vector2(1f, 0f);
            _badgeRt.pivot = new Vector2(1f, 0f);
            _badgeRt.anchoredPosition = new Vector2(-35f, 35f);
            _badgeRt.sizeDelta = new Vector2(180f, 60f);

            _canvasGroup = badgeObj.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            // Autosave Icon
            var iconObj = new GameObject("AutoSaveIcon");
            iconObj.transform.SetParent(badgeObj.transform, false);

            _iconRt = iconObj.AddComponent<RectTransform>();
            _iconRt.anchorMin = new Vector2(0f, 0.5f);
            _iconRt.anchorMax = new Vector2(0f, 0.5f);
            _iconRt.pivot = new Vector2(0.5f, 0.5f);
            _iconRt.anchoredPosition = new Vector2(30f, 0f);
            _iconRt.sizeDelta = new Vector2(52f, 52f);

            _iconImg = iconObj.AddComponent<Image>();
            _iconImg.raycastTarget = false;
            _iconImg.preserveAspect = true;

            // Load sprite
            StartCoroutine(LoadSpriteCoroutine());

            // Label text
            var labelObj = new GameObject("AutoSaveLabel");
            labelObj.transform.SetParent(badgeObj.transform, false);

            var labelRt = labelObj.AddComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 0f);
            labelRt.anchorMax = new Vector2(1f, 1f);
            labelRt.offsetMin = new Vector2(62f, 0f);
            labelRt.offsetMax = Vector2.zero;

            _labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
            _labelTmp.text = "SAVING...";
            _labelTmp.fontSize = 20f;
            _labelTmp.fontStyle = FontStyles.Bold;
            _labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            _labelTmp.color = new Color(0.96f, 0.88f, 0.72f, 1f);
            _labelTmp.raycastTarget = false;
        }

        private IEnumerator LoadSpriteCoroutine()
        {
            var spr = Resources.Load<Sprite>("Icon_AutoSave");
#if UNITY_EDITOR
            if (spr == null)
            {
                spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icon_AutoSave.png");
            }
#endif
            if (spr != null && _iconImg != null)
            {
                _iconImg.sprite = spr;
            }
            yield break;
        }

        public void ShowSaving()
        {
            _isSaving = true;
            if (_displayCoroutine != null) StopCoroutine(_displayCoroutine);
            _displayCoroutine = StartCoroutine(SavingAnimationCoroutine());
        }

        public void OnSaveComplete(bool success)
        {
            _isSaving = false;
        }

        private IEnumerator SavingAnimationCoroutine()
        {
            if (_labelTmp != null)
            {
                _labelTmp.text = "SAVING...";
                _labelTmp.color = new Color(0.96f, 0.88f, 0.72f, 1f);
            }

            // Fade in
            float elapsed = 0f;
            float duration = 0.25f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (_canvasGroup != null) _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;

            // Spin / pulse while saving (at least 1 second minimum display)
            float timer = 0f;
            while (_isSaving || timer < 1.0f)
            {
                timer += Time.unscaledDeltaTime;
                if (_iconRt != null)
                {
                    // Gentle floating rotation and pulse
                    float angle = Mathf.Sin(timer * 6f) * 12f;
                    float scale = 1f + Mathf.Sin(timer * 8f) * 0.08f;
                    _iconRt.localRotation = Quaternion.Euler(0f, 0f, angle);
                    _iconRt.localScale = new Vector3(scale, scale, 1f);
                }
                yield return null;
            }

            // Show saved state
            if (_labelTmp != null)
            {
                _labelTmp.text = "SAVED!";
                _labelTmp.color = new Color(0.45f, 0.95f, 0.45f, 1f);
            }

            if (_iconRt != null)
            {
                _iconRt.localRotation = Quaternion.identity;
                _iconRt.localScale = Vector3.one;
            }

            yield return new WaitForSecondsRealtime(0.8f);

            // Fade out
            elapsed = 0f;
            duration = 0.4f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (_canvasGroup != null) _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
        }
    }
}
