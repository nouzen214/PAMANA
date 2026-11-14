using UnityEngine;
using TMPro;

namespace TopDownGame
{
    /// <summary>
    /// Attach to the Player GameObject (or a child trigger collider on the player).
    /// Detects DoorController objects in range and sends Interact() when the
    /// player presses the Interact button (E / F / Gamepad South / Mobile 'E' Button).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DoorInteractor : MonoBehaviour
    {
        [Header("Interact Prompt (Optional)")]
        [Tooltip("A TMP label to show '[E] Open' when near a door. Leave empty to skip.")]
        [SerializeField] private TMP_Text promptLabel;
        [SerializeField] private string openText  = "[E] Open";
        [SerializeField] private string closeText = "[E] Close";
        [SerializeField] private float interactionRadius = 2.8f;

        // ─── State ────────────────────────────────────────────────────────────
        private DoorController _nearestDoor;
        private PlayerInputHandler _input;
        private int _lastInteractFrame = -1;

        // ─────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            // Walk up to find the root PlayerInputHandler
            _input = GetComponentInParent<PlayerInputHandler>();
            if (_input == null)
            {
                _input = FindFirstObjectByType<PlayerInputHandler>();
            }

            // Make sure our own collider is a trigger
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Update()
        {
            // Continuously verify and update nearest door / interactable within range
            var detected = GetNearestInteractable(interactionRadius);
            if (detected != _nearestDoor)
            {
                _nearestDoor = detected;
                if (promptLabel != null)
                {
                    promptLabel.gameObject.SetActive(_nearestDoor != null);
                }
            }

            if (_nearestDoor != null && promptLabel != null)
            {
                promptLabel.text = _nearestDoor.GetPromptText(openText, closeText);
            }

            // Fire on interact press from keyboard / gamepad / mobile
            if (_input != null && _input.InteractPressed)
            {
                TriggerInteract();
            }
        }

        public DoorController GetNearestInteractable(float maxDistance = 2.8f)
        {
            // 1. If currently tracked door is still within distance, keep it
            if (_nearestDoor != null)
            {
                float curDist = Vector2.Distance(transform.position, _nearestDoor.transform.position);
                if (curDist <= maxDistance)
                {
                    return _nearestDoor;
                }
            }

            DoorController bestDoor = null;
            float bestDist = maxDistance;

            // 2. OverlapCircle check around player
            var hits = Physics2D.OverlapCircleAll(transform.position, maxDistance);
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                var door = hit.GetComponentInParent<DoorController>() ?? hit.GetComponent<DoorController>();
                if (door != null && door.gameObject.activeInHierarchy)
                {
                    float d = Vector2.Distance(transform.position, door.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestDoor = door;
                    }
                }
            }

            // 3. Fallback: Search all active DoorControllers in scene
            if (bestDoor == null)
            {
                var allDoors = Object.FindObjectsByType<DoorController>(FindObjectsSortMode.None);
                foreach (var door in allDoors)
                {
                    if (door == null || !door.gameObject.activeInHierarchy) continue;
                    float d = Vector2.Distance(transform.position, door.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestDoor = door;
                    }
                }
            }

            return bestDoor;
        }

        public void TriggerInteract()
        {
            if (Application.isPlaying && Time.frameCount == _lastInteractFrame) return;
            _lastInteractFrame = Time.frameCount;

            var target = _nearestDoor ?? GetNearestInteractable(interactionRadius);
            if (target != null)
            {
                _nearestDoor = target;
                target.Interact();
            }
        }

        public void SetPromptLabel(TMP_Text label)
        {
            promptLabel = label;
        }

        // ─── Trigger detection ────────────────────────────────────────────────
        private void OnTriggerEnter2D(Collider2D other)
        {
            var door = other.GetComponentInParent<DoorController>() ?? other.GetComponent<DoorController>();
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
            var door = other.GetComponentInParent<DoorController>() ?? other.GetComponent<DoorController>();
            if (door != null && door == _nearestDoor)
            {
                // Do not prematurely null if player is still within interaction radius
                if (Vector2.Distance(transform.position, door.transform.position) > interactionRadius)
                {
                    _nearestDoor = null;
                    if (promptLabel != null)
                        promptLabel.gameObject.SetActive(false);
                }
            }
        }
    }
}
