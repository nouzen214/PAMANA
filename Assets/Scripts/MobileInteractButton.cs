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
        private int _lastTriggerFrame = -1;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null) _originalScale = _rectTransform.localScale;

            // Remove button onClick listeners to prevent double-firing with IPointerDownHandler
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
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
            TriggerInteractAction();
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
            if (Time.frameCount == _lastTriggerFrame) return;
            _lastTriggerFrame = Time.frameCount;

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
            if (player != null)
            {
                var interactor = player.GetComponentInChildren<DoorInteractor>();
                if (interactor != null)
                {
                    interactor.TriggerInteract();
                }
            }
        }
    }
}

