using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    public class HealthUI : MonoBehaviour
    {
        [Header("Target Health")]
        [SerializeField] private Health targetHealth;

        [Header("UI Elements")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Visual Colors")]
        [SerializeField] private Color fullHealthColor = new Color(0.2f, 0.8f, 0.3f);
        [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private float lowHealthThreshold = 0.3f;

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

            if (targetHealth != null)
            {
                BindHealth(targetHealth);
            }
        }

        private void OnDestroy()
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= UpdateUI;
            }
        }

        public void BindHealth(Health health)
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= UpdateUI;
            }

            targetHealth = health;
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged += UpdateUI;
                UpdateUI(targetHealth.CurrentHealth, targetHealth.MaxHealth);
            }
        }

        private void UpdateUI(float current, float max)
        {
            float ratio = max > 0 ? Mathf.Clamp01(current / max) : 0f;

            if (healthSlider != null)
            {
                healthSlider.value = ratio;
            }

            if (fillImage != null)
            {
                fillImage.fillAmount = ratio;
                fillImage.color = ratio <= lowHealthThreshold ? lowHealthColor : fullHealthColor;
            }

            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }
        }
    }
}
