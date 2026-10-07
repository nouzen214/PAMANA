using UnityEngine;

namespace TopDownGame
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float acceleration = 50f;
        [SerializeField] private float deceleration = 50f;

        private Rigidbody2D _rb;
        private PlayerInputHandler _inputHandler;
        private Health _health;
        private Vector2 _currentVelocity;

        public Vector2 LastFacingDirection { get; private set; } = Vector2.down;

        private void Awake()
        {
            EnsureReferences();
            enabled = true;
        }

        private void Start()
        {
            EnsureReferences();
        }

        public void EnsureReferences()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_inputHandler == null) _inputHandler = GetComponent<PlayerInputHandler>();
            if (_health == null) _health = GetComponent<Health>();

            if (_rb != null)
            {
                _rb.gravityScale = 0f;
                _rb.freezeRotation = true;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
        }

        private void OnEnable()
        {
            EnsureReferences();
            if (_health != null)
            {
                _health.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDeath -= HandleDeath;
            }
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        private void FixedUpdate()
        {
            EnsureReferences();

            if (_health != null && _health.IsDead)
            {
                if (_rb != null) _rb.linearVelocity = Vector2.zero;
                return;
            }

            Move();
        }

        private void Move()
        {
            if (_rb == null || _inputHandler == null) return;

            Vector2 targetInput = _inputHandler.MoveInput;
            Vector2 targetVelocity = targetInput * moveSpeed;

            float rate = targetInput.sqrMagnitude > 0.01f ? acceleration : deceleration;
            _currentVelocity = Vector2.MoveTowards(_currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);
            _rb.linearVelocity = _currentVelocity;

            if (targetInput.sqrMagnitude > 0.01f)
            {
                LastFacingDirection = targetInput.normalized;
            }
        }

        private void HandleDeath()
        {
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            enabled = false;
        }

        public void SetSpeed(float newSpeed)
        {
            moveSpeed = Mathf.Max(0f, newSpeed);
        }
    }
}

