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
