using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// Attach to the locked portal barrier in the Lobby.
    /// Provides interaction feedback when the player walks up to the chained portal door before it is awakened.
    /// </summary>
    public class LockedPortalDoor : DoorController
    {
        [Header("Locked Portal Settings")]
        [SerializeField] private string lockedTitle = "LOCKED PORTAL";
        [SerializeField] [TextArea] private string lockedMessage = "This doorway is sealed by heavy chains.\nStudy all 5 historical plaques in Room 1 to awaken the portal!";

        public override string GetPromptText(string openText, string closeText)
        {
            if (GameProgression.IsPortalUnlocked) return string.Empty;
            return "[E] Examine Chained Door";
        }

        public override void Interact()
        {
            if (GameProgression.IsPortalUnlocked) return;

            NotificationBannerUI.Show(
                lockedTitle,
                lockedMessage,
                4.5f,
                new Color(0.95f, 0.55f, 0.25f, 1f),
                "!"
            );
        }
    }
}

