using System.Collections;
using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// Placed on the bottom exit doorway trigger of a Room scene.
    /// Exits back to Lobby when player presses Interact ([E] / Controller / Touch).
    /// Prevents any accidental or instant exit on scene load with an entry grace period.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RoomExit : DoorController
    {
        [Header("Exit Settings")]
        [Tooltip("Prompt displayed to player near exit")]
        [SerializeField] private string exitPrompt = "[E] Exit Room";

        [Tooltip("Grace period in seconds after entering room before exit can be triggered")]
        [SerializeField] private float entryCooldown = 0.8f;

        [Tooltip("If true, walking down into the doorway triggers exit after the cooldown")]
        [SerializeField] private bool autoExitOnWalkDown = false;

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

        public override void Interact()
        {
            if (!_canExit || _isExiting) return;
            PerformExit();
        }

        public override string GetPromptText(string openText, string closeText)
        {
            return exitPrompt;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_canExit || _isExiting) return;

            // Only trigger auto-exit if explicitly enabled AND player is intentionally moving downward
            if (autoExitOnWalkDown && other.CompareTag("Player"))
            {
                var input = other.GetComponent<PlayerInputHandler>();
                if (input != null && input.MoveInput.y < -0.3f)
                {
                    PerformExit();
                }
            }
        }

        public void PerformExit()
        {
            if (_isExiting) return;
            _isExiting = true;
            Debug.Log("[RoomExit] Returning to Lobby...");
            RoomTransitionManager.ExitToLobby();
        }
    }
}
