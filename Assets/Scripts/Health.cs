using System;
using UnityEngine;

namespace TopDownGame
{
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float invulnerabilityDuration = 0.5f;

        private bool _isInitialized;
        private float _currentHealth = 100f;

        public float CurrentHealth
        {
            get => _isInitialized ? _currentHealth : maxHealth;
            private set => _currentHealth = value;
        }

        public float MaxHealth => maxHealth;
        public bool IsDead => _isInitialized && _currentHealth <= 0f;

        public event Action<float, float> OnHealthChanged;
        public event Action<float> OnTakeDamage;
        public event Action OnDeath;

        private float _lastDamageTime = -999f;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _currentHealth = maxHealth;
            _isInitialized = true;
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            if (Time.time < _lastDamageTime + invulnerabilityDuration) return;

            _lastDamageTime = Time.time;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            
            OnTakeDamage?.Invoke(amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0f)
            {
                Die();
            }
            else
            {
                StartCoroutine(DamageFlashRoutine());
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Die()
        {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} died!");
        }

        private System.Collections.IEnumerator DamageFlashRoutine()
        {
            if (_spriteRenderer == null) yield break;

            Color original = _spriteRenderer.color;
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = original;
            }
        }
    }
}
