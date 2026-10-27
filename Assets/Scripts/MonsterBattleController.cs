using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    public class MonsterBattleController : MonoBehaviour
    {
        [Header("Monster Display")]
        [SerializeField] private Image monsterImage;
        [SerializeField] private Image monsterPortraitImage;
        [SerializeField] private Transform monsterPortraitTransform;
        [SerializeField] private Sprite monsterSprite;

        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 6;
        [SerializeField] private int currentHealth = 6;
        [SerializeField] private List<Image> heartImages = new List<Image>();
        [SerializeField] private Sprite heartFullSprite;
        [SerializeField] private Sprite heartEmptySprite;

        [Header("Damage Animation Settings")]
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeMagnitude = 18f;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private TMP_Text floatingDamageText;

        private Vector3 _originalLocalPos;
        private Vector3 _originalLocalScale;
        private Color _originalColor = Color.white;
        private Vector3 _portraitOriginalLocalPos;
        private Color _portraitOriginalColor = Color.white;
        private Coroutine _damageCoroutine;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDefeated => currentHealth <= 0;

        private void Awake()
        {
            if (monsterImage != null)
            {
                _originalLocalPos = monsterImage.transform.localPosition;
                _originalLocalScale = monsterImage.transform.localScale;
                _originalColor = monsterImage.color;
            }
            else
            {
                _originalLocalPos = transform.localPosition;
                _originalLocalScale = transform.localScale;
            }

            if (monsterPortraitTransform != null)
            {
                _portraitOriginalLocalPos = monsterPortraitTransform.localPosition;
            }
            if (monsterPortraitImage != null)
            {
                _portraitOriginalColor = monsterPortraitImage.color;
            }

            currentHealth = maxHealth;
            UpdateHeartUI();
            if (floatingDamageText != null)
            {
                floatingDamageText.gameObject.SetActive(false);
            }
        }

        public void SetMonsterSprite(Sprite sprite)
        {
            monsterSprite = sprite;
            if (monsterImage != null && sprite != null)
            {
                monsterImage.sprite = sprite;
            }
        }

        public void SetupHearts(List<Image> hearts, Sprite full, Sprite empty)
        {
            heartImages = hearts;
            heartFullSprite = full;
            heartEmptySprite = empty;
            UpdateHeartUI();
        }

        public void TakeDamage(int amount, Action onFinished = null)
        {
            currentHealth = Mathf.Max(0, currentHealth - amount);
            UpdateHeartUI();

            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
            }
            _damageCoroutine = StartCoroutine(DamageEffectRoutine(amount, onFinished));
        }

        private IEnumerator DamageEffectRoutine(int amount, Action onFinished)
        {
            Transform targetTr = monsterImage != null ? monsterImage.transform : transform;

            // 1. Show floating damage indicator
            if (floatingDamageText != null)
            {
                floatingDamageText.text = $"-{amount} HP!";
                floatingDamageText.gameObject.SetActive(true);
                floatingDamageText.color = new Color(1f, 0.25f, 0.25f, 1f);
                StartCoroutine(FloatingTextRoutine());
            }

            // 2. Shake, Squash/Stretch Scale, and Flash Red
            float elapsed = 0f;
            int flashCount = 0;
            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float percent = elapsed / shakeDuration;
                float currentMag = Mathf.Lerp(shakeMagnitude, 0f, percent);

                // Horizontal and vertical jitter
                float offsetX = Mathf.Sin(elapsed * 55f) * currentMag;
                float offsetY = Mathf.Cos(elapsed * 40f) * (currentMag * 0.4f);
                targetTr.localPosition = _originalLocalPos + new Vector3(offsetX, offsetY, 0f);

                if (monsterPortraitTransform != null)
                {
                    monsterPortraitTransform.localPosition = _portraitOriginalLocalPos + new Vector3(offsetX * 0.7f, offsetY * 0.7f, 0f);
                }

                // Squash/Stretch scale punch
                float scaleMod = 1f - Mathf.Sin(percent * Mathf.PI) * 0.12f;
                targetTr.localScale = new Vector3(_originalLocalScale.x * (2f - scaleMod), _originalLocalScale.y * scaleMod, _originalLocalScale.z);

                // Rapid flash between damage color and original color
                flashCount = (int)(elapsed * 20f);
                Color currentColor = (flashCount % 2 == 0) ? damageFlashColor : _originalColor;
                if (monsterImage != null)
                {
                    monsterImage.color = currentColor;
                }
                if (monsterPortraitImage != null)
                {
                    monsterPortraitImage.color = (flashCount % 2 == 0) ? damageFlashColor : _portraitOriginalColor;
                }

                yield return null;
            }

            // Reset transform and color
            targetTr.localPosition = _originalLocalPos;
            targetTr.localScale = _originalLocalScale;
            if (monsterImage != null)
            {
                monsterImage.color = _originalColor;
            }
            if (monsterPortraitTransform != null)
            {
                monsterPortraitTransform.localPosition = _portraitOriginalLocalPos;
            }
            if (monsterPortraitImage != null)
            {
                monsterPortraitImage.color = _portraitOriginalColor;
            }

            // 3. If defeated, play defeat fade
            if (IsDefeated)
            {
                yield return StartCoroutine(DefeatFadeRoutine());
            }

            onFinished?.Invoke();
        }

        private IEnumerator FloatingTextRoutine()
        {
            if (floatingDamageText == null) yield break;

            Vector3 startPos = floatingDamageText.transform.localPosition;
            float dur = 0.85f;
            float el = 0f;

            while (el < dur)
            {
                el += Time.deltaTime;
                float p = el / dur;
                floatingDamageText.transform.localPosition = startPos + new Vector3(0f, p * 45f, 0f);
                floatingDamageText.alpha = 1f - p;
                yield return null;
            }

            floatingDamageText.gameObject.SetActive(false);
            floatingDamageText.transform.localPosition = startPos;
        }

        private IEnumerator DefeatFadeRoutine()
        {
            Transform targetTr = monsterImage != null ? monsterImage.transform : transform;
            float dur = 1.0f;
            float el = 0f;

            while (el < dur)
            {
                el += Time.deltaTime;
                float p = el / dur;
                if (monsterImage != null)
                {
                    monsterImage.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 1f - p);
                }
                targetTr.localScale = _originalLocalScale * (1f - p * 0.4f);
                yield return null;
            }

            if (monsterImage != null)
            {
                monsterImage.gameObject.SetActive(false);
            }
        }

        public void UpdateHeartUI()
        {
            if (heartImages == null || heartImages.Count == 0) return;

            for (int i = 0; i < heartImages.Count; i++)
            {
                if (heartImages[i] == null) continue;
                bool isFull = i < currentHealth;
                if (heartFullSprite != null && heartEmptySprite != null)
                {
                    heartImages[i].sprite = isFull ? heartFullSprite : heartEmptySprite;
                }
                heartImages[i].color = isFull ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.45f);
            }
        }
    }
}
