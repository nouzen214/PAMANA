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

        [Header("Scene Transition Destination")]
        [Tooltip("Target scene to load upon teleporting (e.g. AltLobby, Lobby)")]
        [SerializeField] private string targetSceneName = "AltLobby";

        [Tooltip("Spawn position in the target scene")]
        [SerializeField] private Vector2 targetSpawnPosition = new Vector2(0f, 2.5f);

        [Tooltip("Facing direction upon appearing in the target scene")]
        [SerializeField] private CharacterAnimator2D.FacingDirection targetFacing = CharacterAnimator2D.FacingDirection.Down;

        [Header("Teleport Animation")]
        [SerializeField] private float suctionDuration = 0.8f;
        [SerializeField] private float spinRotations = 2f;

        [Header("Events")]
        public UnityEvent onPortalEntered;
        public UnityEvent onTeleportComplete;

        private bool _isTeleporting = false;

        public void SetConfig(string sceneName, Vector2 spawnPos, CharacterAnimator2D.FacingDirection facing)
        {
            targetSceneName = sceneName;
            targetSpawnPosition = spawnPos;
            targetFacing = facing;
        }

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
            Debug.Log($"[PortalController] Player traveled through the Portal to {targetSceneName}!");

            yield return new WaitForSeconds(0.3f);

            if (!string.IsNullOrEmpty(targetSceneName))
            {
                RoomTransitionManager.ReturnSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                RoomTransitionManager.ReturnPosition = targetSpawnPosition;
                RoomTransitionManager.HasReturnPosition = true;
                RoomTransitionManager.ReturnFacing = targetFacing;
                RoomTransitionManager.HasReturnFacing = true;

                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                // Fallback local respawn
                player.transform.position = new Vector3(targetSpawnPosition.x, targetSpawnPosition.y, 0f);
                player.transform.rotation = Quaternion.identity;
                player.transform.localScale = startScale;

                if (rb != null) rb.simulated = true;
                if (col != null) col.enabled = true;
                if (movement != null) movement.enabled = true;

                _isTeleporting = false;
            }
        }
    }
}

