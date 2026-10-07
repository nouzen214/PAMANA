using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDownGame
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Virtual Joystick (Mobile)")]
        [SerializeField] private VirtualJoystick virtualJoystick;

        public Vector2 MoveInput { get; private set; }
        public bool InteractPressed { get; private set; }

        private void Update()
        {
            Vector2 input = Vector2.zero;

            // 1. Primary: New Input System Keyboard (Direct Hardware Polling)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
            }

            // 2. Secondary: Gamepad / Controller
            if (input.sqrMagnitude < 0.01f && Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();
                Vector2 dpad = Gamepad.current.dpad.ReadValue();
                Vector2 padInput = stick.sqrMagnitude > dpad.sqrMagnitude ? stick : dpad;
                if (padInput.sqrMagnitude > 0.04f)
                {
                    input = padInput;
                }
            }

            // 3. Fallback: Legacy Input Manager (W, A, S, D, Arrow keys, Axes)
            if (input.sqrMagnitude < 0.01f)
            {
                try
                {
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.y += 1f;
                    if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.y -= 1f;
                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input.x -= 1f;
                    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x += 1f;

                    if (input.sqrMagnitude < 0.01f)
                    {
                        float h = Input.GetAxisRaw("Horizontal");
                        float v = Input.GetAxisRaw("Vertical");
                        if (Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f)
                        {
                            input = new Vector2(h, v);
                        }
                    }
                }
                catch
                {
                    // Ignore legacy exception if active input handler is exclusively New Input System
                }
            }

            // 4. Mobile / On-Screen Virtual Joystick
            if (virtualJoystick != null && virtualJoystick.InputDirection.sqrMagnitude > 0.01f)
            {
                input = virtualJoystick.InputDirection;
            }

            // Normalize if diagonal exceeds magnitude of 1
            if (input.sqrMagnitude > 1f)
            {
                input = input.normalized;
            }

            MoveInput = input;

            // Interact button: E / F key or Gamepad South (A on Xbox / Cross on PS)
            bool interact = false;
            if (Keyboard.current != null)
                interact = Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame;
            if (!interact && Gamepad.current != null)
                interact = Gamepad.current.buttonSouth.wasPressedThisFrame;
            InteractPressed = interact;
        }

        public void SetVirtualJoystick(VirtualJoystick joystick)
        {
            virtualJoystick = joystick;
        }
    }
}


