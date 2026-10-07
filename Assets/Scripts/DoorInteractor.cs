using UnityEngine;
using TMPro;

namespace TopDownGame
{
    /// <summary>
    /// Attach to the Player GameObject (or a child trigger collider on the player).
    /// Detects DoorController objects in range and sends Interact() when the
    /// player presses the Interact button (E / F / Gamepad South).
    ///
    /// Requires:
    ///   - A trigger Collider2D on this GameObject (set as trigger)
    ///   - PlayerInputHandler on the root player object
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DoorInteractor : MonoBehaviour
    {
        [Header("Interact Prompt (Optional)")]
        [Tooltip("A TMP label to show '[E] Open' when near a door. Leave empty to skip.")]
        [SerializeField] private TMP_Text promptLabel;
        [SerializeField] private string openText  = "[E] Open";
        [SerializeField] private string closeText = "[E] Close";

        // ─── State ────────────────────────────────────────────────────────────
        private DoorController _nearestDoor;
        private PlayerInputHandler _input;

        // ─────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            // Walk up to find the root PlayerInputHandler
            _input = GetComponentInParent<PlayerInputHandler>();

            // Make sure our own collider is a trigger
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Update()
        {
            if (_nearestDoor == null) return;
            if (_input == null) return;

            // Update prompt text
            if (promptLabel != null)
                promptLabel.text = _nearestDoor.GetPromptText(openText, closeText);

            // Fire on interact press
            if (_input.InteractPressed)
                _nearestDoor.Interact();
        }

        public void TriggerInteract()
        {
            if (_nearestDoor != null)
                _nearestDoor.Interact();
        }

        public void SetPromptLabel(TMP_Text label)
        {
            promptLabel = label;
        }

        // ─── Trigger detection ────────────────────────────────────────────────
        private void OnTriggerEnter2D(Collider2D other)
        {
            var door = other.GetComponentInParent<DoorController>();
            if (door == null) door = other.GetComponent<DoorController>();
            if (door != null)
            {
                _nearestDoor = door;
                if (promptLabel != null)
                {
                    promptLabel.gameObject.SetActive(true);
                    promptLabel.text = door.GetPromptText(openText, closeText);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var door = other.GetComponentInParent<DoorController>();
            if (door == null) door = other.GetComponent<DoorController>();
            if (door != null && door == _nearestDoor)
            {
                _nearestDoor = null;
                if (promptLabel != null)
                    promptLabel.gameObject.SetActive(false);
            }
        }
    }
}
