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

            // Direct touch/tap in world space (tap doors to enter, tap artifacts to inspect)
            HandleWorldTapInput();
        }

        private void HandleWorldTapInput()
        {
            // If any modal/dialogue UI is already open, skip world tap
            if (ArtifactInspectionUI.Instance != null && ArtifactInspectionUI.Instance.IsOpen) return;
            if (BookModalUI.Instance != null && BookModalUI.Instance.IsOpen) return;
            var bag = FindFirstObjectByType<BagModalUI>(FindObjectsInactive.Include);
            if (bag != null && bag.IsOpen) return;

            bool tapped = false;
            Vector2 screenPos = Vector2.zero;

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    tapped = true;
                    screenPos = touch.position;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                tapped = true;
                screenPos = Input.mousePosition;
            }

            if (!tapped) return;

            // If tapping UI (joystick, menu buttons, etc.), ignore
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
                if (Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
            }

            var cam = Camera.main ?? FindFirstObjectByType<Camera>();
            if (cam == null) return;

            Vector3 worldPos3 = cam.ScreenToWorldPoint(screenPos);
            Vector2 worldPoint = new Vector2(worldPos3.x, worldPos3.y);

            // 1. Check colliders in radius around tap point (generous touch tolerance for fingers)
            DoorController target = null;
            var hits = Physics2D.OverlapCircleAll(worldPoint, 0.65f);
            float bestTapDist = float.MaxValue;
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                var door = hit.GetComponentInParent<DoorController>() ?? hit.GetComponent<DoorController>();
                if (door != null && door.gameObject.activeInHierarchy)
                {
                    float d = Vector2.Distance(worldPoint, hit.bounds.ClosestPoint(worldPoint));
                    if (d < bestTapDist)
                    {
                        bestTapDist = d;
                        target = door;
                    }
                }
            }

            // 2. Fallback: check SpriteRenderer bounds of active DoorControllers
            if (target == null)
            {
                var allDoors = FindObjectsByType<DoorController>(FindObjectsSortMode.None);
                foreach (var d in allDoors)
                {
                    if (d == null || !d.gameObject.activeInHierarchy) continue;
                    var srs = d.GetComponentsInChildren<SpriteRenderer>();
                    foreach (var sr in srs)
                    {
                        if (sr.bounds.Contains(worldPoint) || Vector2.Distance(sr.bounds.center, worldPoint) < 0.75f)
                        {
                            target = d;
                            break;
                        }
                    }
                    if (target != null) break;
                }
            }

            if (target != null)
            {
                // Measure player distance to interactable
                float dist = Vector2.Distance(transform.position, target.transform.position);
                var cols = target.GetComponentsInChildren<Collider2D>();
                foreach (var c in cols)
                {
                    if (c == null) continue;
                    float d = Vector2.Distance(transform.position, c.bounds.ClosestPoint(transform.position));
                    if (d < dist) dist = d;
                }

                // Make sure character is actually beside the element
                if (dist <= interactionRadius) // 2.8f
                {
                    _nearestDoor = target;
                    target.Interact();
                }
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
