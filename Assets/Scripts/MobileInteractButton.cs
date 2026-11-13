using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TopDownGame
{
    /// <summary>
    /// On-screen Mobile Interaction Button ("E" Button).
    /// Fires PlayerInputHandler.TriggerMobileInteract(), DoorInteractor, and advances inspection dialogues.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MobileInteractButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public static bool WasPressedThisFrame { get; private set; }

        private Button _button;
        private RectTransform _rectTransform;
        private Vector3 _originalScale = Vector3.one;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null) _originalScale = _rectTransform.localScale;

            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
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
                _rectTransform.localScale = _originalScale * 0.92f;
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

        private void OnButtonClicked()
        {
            TriggerInteractAction();
        }

        private void TriggerInteractAction()
        {
            WasPressedThisFrame = true;
            PlayerInputHandler.TriggerMobileInteract();

            // Direct trigger on player's DoorInteractor if available
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

            // Direct trigger on ArtifactInspectionUI dialogue if open
            if (ArtifactInspectionUI.Instance != null && ArtifactInspectionUI.Instance.IsOpen)
            {
                ArtifactInspectionUI.Instance.TriggerAdvanceDialogue();
            }
        }
    }
}
