using UnityEngine;
using System.Runtime.InteropServices;

namespace TopDownGame
{
    /// <summary>
    /// Automatically manages mobile controls (Virtual Joystick & E Interact Button).
    /// Shows controls on phones/tablets/mobile WebGL, and hides them on PC/Desktop.
    /// Also dynamically reveals mobile controls if touch input is detected at runtime.
    /// </summary>
    public class MobileControlsManager : MonoBehaviour
    {
        [Header("Editor Testing")]
        [Tooltip("Check this in Inspector to preview mobile controls in the Unity Editor.")]
        [SerializeField] private bool testInEditor = false;

        [Header("Controlled UI Elements")]
        [Tooltip("The Virtual Joystick GameObject. If left empty, will auto-detect in children.")]
        [SerializeField] private GameObject virtualJoystickGO;
        [Tooltip("The Mobile Interaction Button ('E') GameObject. If left empty, will auto-detect in children.")]
        [SerializeField] private GameObject interactButtonGO;
        [Tooltip("The Book Button RectTransform. Position will adjust so it does not overlap with the E button.")]
        [SerializeField] private RectTransform bookButtonRT;

        public static bool IsMobileActive { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool IsMobileBrowser();
#endif

        private void Awake()
        {
            if (virtualJoystickGO == null)
            {
                var vj = GetComponentInChildren<VirtualJoystick>(true);
                if (vj == null && transform.parent != null)
                {
                    vj = transform.parent.GetComponentInChildren<VirtualJoystick>(true);
                }
                if (vj == null)
                {
                    vj = FindFirstObjectByType<VirtualJoystick>(FindObjectsInactive.Include);
                }
                if (vj != null) virtualJoystickGO = vj.gameObject;
            }

            if (interactButtonGO == null)
            {
                var btn = GetComponentInChildren<MobileInteractButton>(true);
                if (btn == null && transform.parent != null)
                {
                    btn = transform.parent.GetComponentInChildren<MobileInteractButton>(true);
                }
                if (btn == null)
                {
                    btn = FindFirstObjectByType<MobileInteractButton>(FindObjectsInactive.Include);
                }
                if (btn != null) interactButtonGO = btn.gameObject;
            }

            if (bookButtonRT == null)
            {
                var bb = transform.Find("BookButton");
                if (bb == null && transform.parent != null) bb = transform.parent.Find("BookButton");
                if (bb == null)
                {
                    var found = GameObject.Find("BookButton");
                    if (found != null) bb = found.transform;
                }
                if (bb != null) bookButtonRT = bb.GetComponent<RectTransform>();
            }

            IsMobileActive = CheckIfMobile();
            ApplyVisibility(IsMobileActive);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (testInEditor != IsMobileActive)
            {
                IsMobileActive = testInEditor;
                ApplyVisibility(IsMobileActive);
            }
#endif

            // Dynamic touch detection: if hidden on hybrid/touchscreen laptops or tablets,
            // the moment the player touches the screen, wake up mobile controls!
            if (!IsMobileActive && Input.touchCount > 0)
            {
                IsMobileActive = true;
                ApplyVisibility(true);
            }
        }

        public void ApplyVisibility(bool isVisible)
        {
            if (virtualJoystickGO != null)
            {
                virtualJoystickGO.SetActive(isVisible);
            }
            if (interactButtonGO != null)
            {
                interactButtonGO.SetActive(isVisible);
            }
            if (bookButtonRT != null)
            {
                bookButtonRT.anchoredPosition = isVisible ? new Vector2(-240f, 110f) : new Vector2(-120f, 120f);
            }
        }

        public bool CheckIfMobile()
        {
#if UNITY_EDITOR
            return testInEditor;
#elif UNITY_WEBGL
            try
            {
                return IsMobileBrowser();
            }
            catch
            {
                string os = SystemInfo.operatingSystem.ToLowerInvariant();
                return os.Contains("iphone") || os.Contains("ipad") || os.Contains("android") || os.Contains("mobile");
            }
#else
            if (Application.isMobilePlatform) return true;
            if (SystemInfo.deviceType == DeviceType.Handheld) return true;
            string os = SystemInfo.operatingSystem.ToLowerInvariant();
            if (os.Contains("android") || os.Contains("iphone") || os.Contains("ipad") || os.Contains("ios")) return true;
            return false;
#endif
        }
    }
}
