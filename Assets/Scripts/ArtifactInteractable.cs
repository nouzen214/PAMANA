using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// Attached to each display table or artifact in the room.
    /// Inherits from DoorController so Player's DoorInteractor automatically detects it.
    /// Pressing interact opens the artifact inspection dialogue with choices.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ArtifactInteractable : DoorController
    {
        [Header("Artifact Information")]
        [SerializeField] private string artifactName = "Ancient Artifact";
        [SerializeField] private Sprite artifactSprite;
        [SerializeField] private string plaqueTitle = "...";
        [TextArea(2, 5)]
        [SerializeField] private string plaqueDescription = "An ancient artifact with mysterious Philippine inscriptions.";
        [TextArea(2, 5)]
        [SerializeField] private string artifactInspectionDescription = "Exquisite craftsmanship preserved through the centuries.";

        [Header("Character Dialogue")]
        [SerializeField] private string initialDialogue = "what should i do";
        [SerializeField] private string plaqueDialogue = "what could this artifact be";
        [SerializeField] private string artifactDialogue = "i wonder what this artifact looks like";

        [Header("Interaction Prompt")]
        [SerializeField] private string promptText = "[E] Inspect Artifact";

        public string ArtifactName => artifactName;
        public Sprite ArtifactSprite => artifactSprite;
        public string PlaqueTitle => plaqueTitle;
        public string PlaqueDescription => plaqueDescription;
        public string ArtifactInspectionDescription => artifactInspectionDescription;
        public string InitialDialogue => initialDialogue;
        public string PlaqueDialogue => plaqueDialogue;
        public string ArtifactDialogue => artifactDialogue;

        public override void Interact()
        {
            // Check if interacting in Room 1
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName == "Room_1" || RoomTransitionManager.CurrentRoomNumber == 1)
            {
                if (GameProgression.HasRetrievedRoom1Artifact && !GameProgression.HasReturnedRoom1ArtifactToRealWorld)
                {
                    bool justReturned = GameProgression.ReturnRoom1ArtifactToRealWorld();
                    if (justReturned)
                    {
                        NotificationBannerUI.Show(
                            "ARTIFACTS RETURNED TO ROOM 1!",
                            "You brought the artifacts back inside Room 1! Alterworld Room 2 is now UNLOCKED!",
                            6.0f,
                            new Color(0.2f, 0.95f, 0.4f, 1f),
                            "★"
                        );
                    }
                }

                if (!GameProgression.IsPortalUnlocked)
                {
                    bool justUnlocked = GameProgression.UnlockPortal();
                    if (justUnlocked)
                    {
                        NotificationBannerUI.Show(
                            "PORTAL OPENED",
                            "A mysterious portal has opened somewhere in the museum!",
                            5.5f,
                            new Color(0.2f, 0.9f, 0.9f, 1f),
                            "!"
                        );
                    }
                }
            }

            var ui = ArtifactInspectionUI.Instance;
            if (ui == null)
            {
                ui = FindFirstObjectByType<ArtifactInspectionUI>(FindObjectsInactive.Include);
            }

            if (ui != null)
            {
                ui.Open(this);
            }
            else
            {
                Debug.LogWarning("[ArtifactInteractable] ArtifactInspectionUI instance not found in scene!");
            }
        }

        public override string GetPromptText(string openText, string closeText)
        {
            return promptText;
        }

        public void Configure(string name, Sprite sprite, string title, string plaqueDesc, string inspectDesc, string initialDiag, string reactionDiag, string artDiag = "i wonder what this artifact looks like")
        {
            artifactName = name;
            artifactSprite = sprite;
            plaqueTitle = title;
            plaqueDescription = plaqueDesc;
            artifactInspectionDescription = inspectDesc;
            initialDialogue = initialDiag;
            plaqueDialogue = reactionDiag;
            artifactDialogue = artDiag;
        }
    }
}
