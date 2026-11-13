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

            int count = GameProgression.GetPlaquesReadCount(1);
            bool plaquesDone = GameProgression.AreAllPlaquesRead(1);
            bool topicDone = GameProgression.IsBookTopicReviewed(1);

            string msg;
            if (!plaquesDone && !topicDone)
            {
                msg = $"This doorway is sealed by heavy chains.\nStudy all 5 plaques in Room 1 ({count}/5 studied) and review Topic 1 in your Field Guide to awaken the portal!";
            }
            else if (!plaquesDone && topicDone)
            {
                msg = $"You reviewed Topic 1 in the Field Guide!\nNow study all 5 historical plaques in Room 1 ({count}/5 studied) to awaken the portal!";
            }
            else if (plaquesDone && !topicDone)
            {
                msg = "All 5 plaques in Room 1 studied! But the ancient portal remains asleep.\nOpen your Museum Field Guide (Book icon / [H]) and review Topic 1 to awaken the portal!";
            }
            else
            {
                msg = "The portal seal is breaking! Open your Field Guide or enter Room 1 to awaken the portal!";
            }

            NotificationBannerUI.Show(
                lockedTitle,
                msg,
                5.0f,
                new Color(0.95f, 0.55f, 0.25f, 1f),
                "!"
            );
        }
    }
}

