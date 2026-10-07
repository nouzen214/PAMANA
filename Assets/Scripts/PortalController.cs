using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace TopDownGame
{
    [RequireComponent(typeof(Collider2D))]
    public class PortalController : MonoBehaviour
    {
        [Header("Activation Conditions")]
        [Tooltip("Require player to be moving upwards (WASD W or Joystick Up) to enter")]
        [SerializeField] private bool requireUpwardMovement = true;

        [Header("Teleport Animation")]
        [SerializeField] private float suctionDuration = 0.8f;
        [SerializeField] private float spinRotations = 2f;

        [Header("Events")]
        public UnityEvent onPortalEntered;
        public UnityEvent onTeleportComplete;

        private bool _isTeleporting = false;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (_isTeleporting) return;

            if (other.CompareTag("Player"))
            {
                var inputHandler = other.GetComponent<PlayerInputHandler>();
                
                // Check if player is pressing upwards or if upward check is bypassed
                bool isMovingUp = inputHandler == null || inputHandler.MoveInput.y > 0.1f;

                if (!requireUpwardMovement || isMovingUp)
                {
                    StartCoroutine(TeleportRoutine(other.gameObject));
                }
            }
        }

        private IEnumerator TeleportRoutine(GameObject player)
        {
            _isTeleporting = true;
            onPortalEntered?.Invoke();

            // Disable player control during teleport
            var movement = player.GetComponent<PlayerMovement>();
            var rb = player.GetComponent<Rigidbody2D>();
            var col = player.GetComponent<Collider2D>();

            if (movement != null) movement.enabled = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
            if (col != null) col.enabled = false;

            Vector3 startPos = player.transform.position;
            Vector3 targetPos = transform.position;
            Vector3 startScale = player.transform.localScale;

            float elapsed = 0f;
            while (elapsed < suctionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / suctionDuration);
                float easeIn = t * t;

                // Move towards portal center
                player.transform.position = Vector3.Lerp(startPos, targetPos, easeIn);

                // Spin and shrink into vortex
                player.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, easeIn);
                player.transform.Rotate(0f, 0f, 360f * spinRotations * Time.deltaTime / suctionDuration);

                yield return null;
            }

            player.transform.localScale = Vector3.zero;
            onTeleportComplete?.Invoke();
            Debug.Log("Player successfully traveled through the Portal!");

            // Brief pause, then reset or transition
            yield return new WaitForSeconds(0.5f);
            
            // Re-spawn or re-enable for now so player can keep playing/testing
            player.transform.position = new Vector3(0f, -2.5f, 0f);
            player.transform.rotation = Quaternion.identity;
            player.transform.localScale = startScale;

            if (rb != null) rb.simulated = true;
            if (col != null) col.enabled = true;
            if (movement != null) movement.enabled = true;

            _isTeleporting = false;
        }
    }
}
