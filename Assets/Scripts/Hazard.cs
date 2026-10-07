using UnityEngine;

namespace TopDownGame
{
    public class Hazard : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private float damageAmount = 20f;
        [SerializeField] private bool damageOverTime = true;
        [SerializeField] private float damageInterval = 1f;

        private float _lastDamageTime;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent<Health>(out var health))
            {
                if (damageOverTime)
                {
                    if (Time.time >= _lastDamageTime + damageInterval)
                    {
                        health.TakeDamage(damageAmount);
                        _lastDamageTime = Time.time;
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!damageOverTime && other.TryGetComponent<Health>(out var health))
            {
                health.TakeDamage(damageAmount);
            }
        }
    }
}
