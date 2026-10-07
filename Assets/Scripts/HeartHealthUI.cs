using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TopDownGame
{
    public class HeartHealthUI : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Health targetHealth;

        [Header("Heart Sprites")]
        [SerializeField] private Sprite fullHeartSprite;
        [SerializeField] private Sprite emptyHeartSprite;

        [Header("Heart Configuration")]
        [Tooltip("Number of hearts to display")]
        [SerializeField] private int totalHearts = 5;
        [SerializeField] private Vector2 heartSize = new Vector2(56f, 56f);
        [SerializeField] private float spacing = 12f;

        private readonly List<Image> _heartImages = new List<Image>();

        private void Start()
        {
            if (targetHealth == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    targetHealth = player.GetComponent<Health>();
                }
            }

            SetupHearts();

            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged += UpdateHearts;
                targetHealth.OnTakeDamage += HandleDamageEffect;
                UpdateHearts(targetHealth.CurrentHealth, targetHealth.MaxHealth);
            }
        }

        private void OnDestroy()
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= UpdateHearts;
                targetHealth.OnTakeDamage -= HandleDamageEffect;
            }
        }

        public void SetupHearts()
        {
            // Clear existing heart children
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            _heartImages.Clear();

            // Setup HorizontalLayoutGroup or anchor manually
            var layout = GetComponent<HorizontalLayoutGroup>();
            if (layout == null) layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            for (int i = 0; i < totalHearts; i++)
            {
                var heartGo = new GameObject($"Heart_{i}");
                heartGo.transform.SetParent(transform, false);

                var rect = heartGo.AddComponent<RectTransform>();
                rect.sizeDelta = heartSize;

                var img = heartGo.AddComponent<Image>();
                img.sprite = fullHeartSprite;
                img.preserveAspect = true;

                _heartImages.Add(img);
            }
        }

        public void UpdateHearts(float currentHp, float maxHp)
        {
            if (maxHp <= 0 || _heartImages.Count == 0) return;

            float hpPercent = Mathf.Clamp01(currentHp / maxHp);
            float activeHearts = hpPercent * totalHearts;

            for (int i = 0; i < _heartImages.Count; i++)
            {
                if (i < Mathf.CeilToInt(activeHearts))
                {
                    _heartImages[i].sprite = fullHeartSprite;
                    _heartImages[i].color = Color.white;
                }
                else
                {
                    _heartImages[i].sprite = emptyHeartSprite;
                    _heartImages[i].color = Color.white;
                }
            }
        }

        private void HandleDamageEffect(float amount)
        {
            // Subtle punch / shake on the hearts container
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine());
        }

        private System.Collections.IEnumerator ShakeRoutine()
        {
            Vector3 originalPos = transform.localPosition;
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float xOffset = Random.Range(-4f, 4f);
                float yOffset = Random.Range(-4f, 4f);
                transform.localPosition = originalPos + new Vector3(xOffset, yOffset, 0f);
                yield return null;
            }

            transform.localPosition = originalPos;
        }

        public void SetTargetHealth(Health health)
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= UpdateHearts;
                targetHealth.OnTakeDamage -= HandleDamageEffect;
            }

            targetHealth = health;
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged += UpdateHearts;
                targetHealth.OnTakeDamage += HandleDamageEffect;
                UpdateHearts(targetHealth.CurrentHealth, targetHealth.MaxHealth);
            }
        }
    }
}
