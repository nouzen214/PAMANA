using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// Manages the activation state of the Lobby Portal to the Alterworld.
    /// The portal remains hidden and inactive until the player interacts with a Room 1 artifact.
    /// </summary>
    public class LobbyPortalManager : MonoBehaviour
    {
        [Tooltip("The Portal GameObject in the Lobby scene. If null, finds 'Portal' by name.")]
        [SerializeField] private GameObject portalObject;

        private void Awake()
        {
            if (portalObject == null)
            {
                portalObject = GameObject.Find("Portal");
            }
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

        public void UpdatePortalState()
        {
            if (portalObject == null)
            {
                portalObject = GameObject.Find("Portal");
            }

            if (portalObject != null)
            {
                bool shouldBeActive = GameProgression.IsPortalUnlocked;
                portalObject.SetActive(shouldBeActive);
                Debug.Log($"[LobbyPortalManager] Portal active state set to {shouldBeActive} (IsPortalUnlocked: {GameProgression.IsPortalUnlocked})");
            }
        }
    }
}
