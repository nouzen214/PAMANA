using System.Collections;
using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// Attached to the bottom exit doorway trigger of each Room scene (Room_1 to Room_12).
    /// When the player walks through the doorway, automatically exits back to the hallway/lobby.
    /// Pure walkthrough trigger — no [E] prompt or interaction button required.
    /// Includes a brief cooldown after entering the room to prevent accidental instant bounce-back.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RoomExit : MonoBehaviour
    {
        [Header("Exit Settings")]
        [Tooltip("Grace period in seconds after entering room before exit trigger activates")]
        [SerializeField] private float entryCooldown = 0.5f;

        private bool _isExiting = false;
        private bool _canExit = false;

        private void Start()
        {
            StartCoroutine(EnableExitCooldownRoutine());
        }

        private IEnumerator EnableExitCooldownRoutine()
        {
            _canExit = false;
            yield return new WaitForSeconds(entryCooldown);
            _canExit = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryExit(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryExit(other);
        }

        private void TryExit(Collider2D other)
        {
            if (!_canExit || _isExiting) return;

            if (other.CompareTag("Player") || other.gameObject.name == "Player")
            {
                PerformExit();
            }
        }

        public void PerformExit()
        {
            if (_isExiting) return;
            _isExiting = true;
            Debug.Log("[RoomExit] Player walked through exit doorway. Returning to hallway/lobby...");
            RoomTransitionManager.ExitToLobby();
        }
    }
}
