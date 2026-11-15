using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TopDownGame
{
    /// <summary>
    /// Prevents keyboard A/D or arrow keys from altering UI slider values.
    /// Sliders will only respond to direct mouse click/drag or touch interaction.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class SliderNoKeyboardNavigation : MonoBehaviour, IMoveHandler, ISelectHandler, IPointerUpHandler, IPointerDownHandler
    {
        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            if (_slider != null)
            {
                var nav = _slider.navigation;
                nav.mode = Navigation.Mode.None;
                _slider.navigation = nav;
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            // Fully consume the move event so Slider.OnMove is NEVER triggered by A/D or Arrow keys
            eventData.Use();
        }

        public void OnSelect(BaseEventData eventData)
        {
            // Deselect immediately so this slider never retains keyboard focus
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Do not allow the slider to retain keyboard focus
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Ensure focus is cleared after drag/click finishes
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}

