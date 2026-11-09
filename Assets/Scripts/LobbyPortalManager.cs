using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// Manages the activation state of the Lobby Portal to the Alterworld.
    /// When the portal is locked, a physical barrier blocks player entry into the chained doorway.
    /// When unlocked, the barrier is disabled and the glowing portal vortex is activated.
    /// </summary>
    public class LobbyPortalManager : MonoBehaviour
    {
        [Tooltip("The Portal GameObject in the Lobby scene. If null, finds 'Portal' by name.")]
        [SerializeField] private GameObject portalObject;

        [Tooltip("The physical barrier blocking the portal doorway while locked. If null, finds or creates 'Lobby_PortalBlocker'.")]
        [SerializeField] private GameObject portalBlocker;

        private void Awake()
        {
            if (portalObject == null)
            {
                var roots = gameObject.scene.GetRootGameObjects();
                foreach (var r in roots)
                {
                    if (r.name == "Portal") { portalObject = r; break; }
                    var tr = r.transform.Find("Portal");
                    if (tr != null) { portalObject = tr.gameObject; break; }
                }
                if (portalObject == null) portalObject = GameObject.Find("Portal");
            }

            EnsurePortalBlocker();
        }

        private void Start()
        {
            UpdatePortalState();
            GameProgression.OnPortalUnlocked += UpdatePortalState;
        }

        private void OnDestroy()
        {
            GameProgression.OnPortalUnlocked -= UpdatePortalState;
        }

        public GameObject EnsurePortalBlocker()
        {
            if (portalBlocker == null)
            {
                portalBlocker = GameObject.Find("Lobby_PortalBlocker");
            }

            if (portalBlocker == null)
            {
                var walls = GameObject.Find("RoomWalls");
                Transform parentTr = walls != null ? walls.transform : null;

                portalBlocker = new GameObject("Lobby_PortalBlocker");
                if (parentTr != null) portalBlocker.transform.SetParent(parentTr, false);
                portalBlocker.transform.position = Vector3.zero;

                // 1. Solid collision barrier to prevent entering the chained door alcove
                var solidCol = portalBlocker.AddComponent<BoxCollider2D>();
                solidCol.offset = new Vector2(0f, 3.28f);
                solidCol.size = new Vector2(3.50f, 1.95f);
                solidCol.isTrigger = false;

                // 2. Interaction trigger for examining the chained door
                var triggerObj = new GameObject("InteractTrigger", typeof(BoxCollider2D), typeof(LockedPortalDoor));
                triggerObj.transform.SetParent(portalBlocker.transform, false);
                var trigCol = triggerObj.GetComponent<BoxCollider2D>();
                trigCol.offset = new Vector2(0f, 2.30f);
                trigCol.size = new Vector2(3.20f, 1.20f);
                trigCol.isTrigger = true;
            }

            return portalBlocker;
        }

        public void UpdatePortalState()
        {
            if (portalObject == null)
            {
                portalObject = GameObject.Find("Portal");
            }

            EnsurePortalBlocker();

            bool isUnlocked = GameProgression.IsPortalUnlocked;

            if (portalObject != null)
            {
                portalObject.SetActive(isUnlocked);
            }

            if (portalBlocker != null)
            {
                portalBlocker.SetActive(!isUnlocked);
            }

            Debug.Log($"[LobbyPortalManager] Portal active state set to {isUnlocked}, Blocker active: {!isUnlocked} (IsPortalUnlocked: {isUnlocked})");
        }
    }
}
