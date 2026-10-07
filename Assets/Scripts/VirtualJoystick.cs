using UnityEngine;
using UnityEngine.EventSystems;

namespace TopDownGame
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Joystick Components")]
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;

        [Header("Settings")]
        [SerializeField] private float handleRange = 60f;

        public Vector2 InputDirection { get; private set; } = Vector2.zero;

        private Canvas _canvas;
        private Camera _uiCamera;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _uiCamera = _canvas.worldCamera;
            }

            if (background == null)
            {
                background = GetComponent<RectTransform>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background,
                    eventData.position,
                    _uiCamera,
                    out position))
            {
                position = Vector2.ClampMagnitude(position, handleRange);
                if (handle != null)
                {
                    handle.anchoredPosition = position;
                }

                InputDirection = position / handleRange;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            InputDirection = Vector2.zero;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }

        public void ResetJoystick()
        {
            InputDirection = Vector2.zero;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }
    }
}
