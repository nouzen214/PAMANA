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
        [SerializeField] private string initialDialogue = "What should I do?";
        [SerializeField] private string plaqueDialogue = "What could this artifact be?";
        [SerializeField] private string artifactDialogue = "I wonder what this artifact looks like...";

        [Header("Interaction Prompt")]
        [SerializeField] private string promptText = "[E] Inspect Artifact";

        [Header("Room Info")]
        [SerializeField] private int roomNumber = 1;

        public int RoomNumber
        {
            get
            {
                if (roomNumber <= 0)
                {
                    string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    if (sceneName.StartsWith("Room_") && int.TryParse(sceneName.Substring(5), out int num))
                    {
                        roomNumber = num;
                    }
                    else
                    {
                        roomNumber = RoomTransitionManager.CurrentRoomNumber > 0 ? RoomTransitionManager.CurrentRoomNumber : 1;
                    }
                }
                return roomNumber;
            }
        }

        public string ArtifactName => artifactName;
        public Sprite ArtifactSprite => artifactSprite;
        public string PlaqueTitle => plaqueTitle;
        public string PlaqueDescription => plaqueDescription;
        public string ArtifactInspectionDescription => artifactInspectionDescription;
        public string InitialDialogue => FormatDialogueSentence(initialDialogue);
        public string PlaqueDialogue => FormatDialogueSentence(plaqueDialogue);
        public string ArtifactDialogue => FormatDialogueSentence(artifactDialogue);

        public static string FormatDialogueSentence(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            string trimmed = text.Trim();

            // Special known dialogue
            if (string.Equals(trimmed, "what could this artifact be", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "what could this artifact be?", System.StringComparison.OrdinalIgnoreCase))
                return "What could this artifact be?";

            if (string.Equals(trimmed, "what should i do", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "what should i do?", System.StringComparison.OrdinalIgnoreCase))
                return "What should I do?";

            if (string.Equals(trimmed, "i wonder what this artifact looks like", System.StringComparison.OrdinalIgnoreCase))
                return "I wonder what this artifact looks like...";

            // Ensure first letter is capitalized
            if (char.IsLower(trimmed[0]))
            {
                trimmed = char.ToUpper(trimmed[0]) + (trimmed.Length > 1 ? trimmed.Substring(1) : "");
            }

            // Replace lowercase isolated 'i' with 'I'
            trimmed = System.Text.RegularExpressions.Regex.Replace(trimmed, @"\bi\b", "I");

            // If question word and missing punctuation, append ?
            string lower = trimmed.ToLowerInvariant();
            if (lower.StartsWith("what ") || lower.StartsWith("why ") || lower.StartsWith("how ") ||
                lower.StartsWith("who ") || lower.StartsWith("where ") || lower.StartsWith("when ") ||
                lower.StartsWith("could ") || lower.StartsWith("would ") || lower.StartsWith("is ") ||
                lower.StartsWith("can "))
            {
                if (!trimmed.EndsWith("?") && !trimmed.EndsWith(".") && !trimmed.EndsWith("!"))
                {
                    trimmed += "?";
                }
            }

            return trimmed;
        }

        public bool IsRestored => GameProgression.IsPedestalRestored(RoomNumber, artifactName);
        public string DisplayName => IsRestored ? artifactName : "???";
        public string DisplayPlaqueTitle => IsRestored ? plaqueTitle : "???";

        private SpriteRenderer _cachedVisualRenderer;

        public SpriteRenderer GetVisualRenderer()
        {
            if (_cachedVisualRenderer != null) return _cachedVisualRenderer;
            var root = GameObject.Find("RoomArtifacts");
            if (root != null)
            {
                float minDist = float.MaxValue;
                SpriteRenderer best = null;
                foreach (Transform child in root.transform)
                {
                    var sr = child.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        float d = Vector2.Distance(child.position, transform.position);
                        if (d < minDist && d < 2.0f)
                        {
                            minDist = d;
                            best = sr;
                        }
                    }
                }
                _cachedVisualRenderer = best;
            }
            return _cachedVisualRenderer;
        }

        public void SetRestoredVisual(bool restored)
        {
            var sr = GetVisualRenderer();
            if (sr != null)
            {
                sr.color = restored ? Color.white : Color.black;
            }
        }

        public override void Interact()
        {
            var ui = ArtifactInspectionUI.Instance;
            if (ui == null)
            {
                ui = FindFirstObjectByType<ArtifactInspectionUI>(FindObjectsInactive.Include);
            }
            if (ui == null)
            {
                var canvas = GameObject.Find("InGameCanvas") ?? GameObject.Find("Canvas");
                if (canvas != null)
                {
                    ui = canvas.GetComponent<ArtifactInspectionUI>() ?? canvas.GetComponentInChildren<ArtifactInspectionUI>(true);
                }
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

        protected override void OnMouseDown()
        {
            base.OnMouseDown();
        }

        public void SetRoomNumber(int r)
        {
            roomNumber = r;
        }

        public void Configure(string name, Sprite sprite, string title, string plaqueDesc, string inspectDesc, string initialDiag, string reactionDiag, string artDiag = "I wonder what this artifact looks like...", int rNum = 1)
        {
            artifactName = name;
            artifactSprite = sprite;
            plaqueTitle = string.IsNullOrEmpty(title) || title == "..." ? name : title;
            plaqueDescription = plaqueDesc;
            artifactInspectionDescription = inspectDesc;
            initialDialogue = initialDiag;
            plaqueDialogue = reactionDiag;
            artifactDialogue = artDiag;
            roomNumber = rNum;
        }
    }
}
