using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TopDownGame
{
    /// <summary>
    /// On-screen Mobile Interaction Button ("E" Button).
    /// Fires PlayerInputHandler.TriggerMobileInteract(), DoorInteractor, and advances inspection dialogues.
    /// Dedicated to the bottom-right right-thumb zone for maximum comfort and responsiveness.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MobileInteractButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public static bool WasPressedThisFrame { get; private set; }

        private Button _button;
        private RectTransform _rectTransform;
        private Vector3 _originalScale = Vector3.one;
        private float _lastTriggerTime = -1f;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null) _originalScale = _rectTransform.localScale;

            // Wire onClick listener debounced alongside pointer down
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() =>
                {
                    if (CanTrigger())
                    {
                        TriggerInteractAction();
                    }
                });
            }
        }

        private bool CanTrigger()
        {
            if (Time.unscaledTime - _lastTriggerTime < 0.12f) return false;
            _lastTriggerTime = Time.unscaledTime;
            return true;
        }

        private void LateUpdate()
        {
            WasPressedThisFrame = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_rectTransform != null)
            {
                _rectTransform.localScale = _originalScale * 0.90f;
            }
            if (CanTrigger())
            {
                TriggerInteractAction();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_rectTransform != null)
            {
                _rectTransform.localScale = _originalScale;
            }
        }

        public void TriggerInteractAction()
        {
            WasPressedThisFrame = true;
            PlayerInputHandler.TriggerMobileInteract();

            // 1. If an inspection dialogue or detail popup is open, advance or return to choice menu
            if (ArtifactInspectionUI.Instance != null && ArtifactInspectionUI.Instance.IsOpen)
            {
                ArtifactInspectionUI.Instance.TriggerAdvanceDialogue();
                return;
            }

            // 2. Otherwise trigger interaction with the nearest artifact pedestal or door
            var player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player == null)
            {
                var pih = FindFirstObjectByType<PlayerInputHandler>();
                if (pih != null) player = pih.gameObject;
            }

            bool interacted = false;
            if (player != null)
            {
                var interactor = player.GetComponentInChildren<DoorInteractor>();
                if (interactor != null)
                {
                    interactor.TriggerInteract();
                    if (ArtifactInspectionUI.Instance != null && ArtifactInspectionUI.Instance.IsOpen)
                    {
                        interacted = true;
                    }
                }
            }

            // 3. Fallback: If not opened yet, scan directly for closest DoorController / Artifact within 3.0 units
            if (!interacted && (ArtifactInspectionUI.Instance == null || !ArtifactInspectionUI.Instance.IsOpen))
            {
                Vector2 searchPos = player != null ? (Vector2)player.transform.position : Vector2.zero;
                DoorController closest = null;
                float closestDist = 3.0f;

                var doors = FindObjectsByType<DoorController>(FindObjectsSortMode.None);
                foreach (var d in doors)
                {
                    if (d == null || !d.gameObject.activeInHierarchy) continue;
                    float dist = Vector2.Distance(searchPos, d.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = d;
                    }
                }

                if (closest != null)
                {
                    closest.Interact();
                }
            }
        }
    }
}

