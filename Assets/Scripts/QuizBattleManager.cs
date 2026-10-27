using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace TopDownGame
{
    [System.Serializable]
    public class ArtifactBattleSlot
    {
        public string artifactId;
        public string artifactName;
        public Sprite artifactSprite;
        [TextArea(2, 3)] public string artifactDescription;
        [TextArea(2, 4)] public string questionText;
        public string[] choices = new string[4];
        public int correctChoiceIndex = 0;
        [HideInInspector] public bool isRetrieved = false;
        [HideInInspector] public bool isFailed = false;
    }

    public class QuizBattleManager : MonoBehaviour
    {
        public static QuizBattleManager Instance { get; private set; }

        [Header("Room & Battle Settings")]
        [SerializeField] private int roomNumber = 1;
        [SerializeField] private string roomTitle = "Room 1 (Alter Realm)";

        public int RoomNumber => roomNumber;
        public string RoomTitle => roomTitle;

        [Header("Monster")]
        [SerializeField] private MonsterBattleController monsterController;

        [Header("Artifacts in Room 1")]
        [SerializeField] private List<ArtifactBattleSlot> artifactSlots = new List<ArtifactBattleSlot>();
        [SerializeField] private List<Button> artifactSlotButtons = new List<Button>();
        [SerializeField] private List<Image> artifactSlotImages = new List<Image>();
        [SerializeField] private List<GameObject> artifactSelectionBorders = new List<GameObject>();
        [SerializeField] private List<GameObject> artifactCheckmarkIcons = new List<GameObject>();

        [Header("Top HUD")]
        [SerializeField] private TMP_Text artifactCounterText;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button bagButton;
        [SerializeField] private BagModalUI bagModal;

        [Header("Player Status (Bottom-Left)")]
        [SerializeField] private Image playerAvatarImage;
        [SerializeField] private Sprite boyAvatarSprite;
        [SerializeField] private Sprite girlAvatarSprite;
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private List<Image> playerHeartImages = new List<Image>();
        [SerializeField] private Sprite heartFullSprite;
        [SerializeField] private Sprite heartEmptySprite;
        [SerializeField] private int playerMaxHealth = 3;
        [SerializeField] private int playerCurrentHealth = 3;
        [SerializeField] private Transform playerPortraitTransform;

        [Header("Dialogue & Quiz UI (Top - Monster Question)")]
        [SerializeField] private GameObject topDialogueBox;
        [SerializeField] private GameObject monsterPortraitContainer;
        [SerializeField] private TMP_Text questionSpeakerText;
        [SerializeField] private TMP_Text questionBodyText;
        [SerializeField] private string monsterSpeakerName = "Manang";

        [Header("Dialogue UI (Bottom - Player Thought)")]
        [SerializeField] private TMP_Text playerSpeakerText;
        [SerializeField] private TMP_Text playerDialogueText;

        [Header("Legacy Dialogue Fallbacks")]
        [SerializeField] private TMP_Text dialogueSpeakerText;
        [SerializeField] private TMP_Text dialogueBodyText;
        [SerializeField] private GameObject choicesContainer;
        [SerializeField] private List<Button> choiceButtons = new List<Button>();
        [SerializeField] private List<TMP_Text> choiceButtonTexts = new List<TMP_Text>();

        [Header("Victory & Defeat Modals")]
        [SerializeField] private GameObject victoryModal;
        [SerializeField] private Button victoryReturnButton;
        [SerializeField] private GameObject defeatModal;
        [SerializeField] private Button defeatRetryButton;
        [SerializeField] private Button defeatExitButton;

        [Header("Settings / Pause Modal")]
        [SerializeField] private GameObject settingsModal;
        [SerializeField] private Button settingsCloseButton;
        [SerializeField] private Button settingsExitHallwayButton;

        private int _selectedSlotIndex = -1;
        private bool _isBusy = false;
        private int _retrievedCount = 0;

        private void Awake()
        {
            Instance = this;

            if (topDialogueBox == null && questionBodyText != null)
            {
                Transform parent = questionBodyText.transform.parent;
                while (parent != null && parent.name != "BattleCanvas")
                {
                    if (parent.name == "TopDialogueBox")
                    {
                        topDialogueBox = parent.gameObject;
                        break;
                    }
                    parent = parent.parent;
                }
            }
            if (topDialogueBox == null)
            {
                var t = transform.Find("TopDialogueBox");
                if (t != null) topDialogueBox = t.gameObject;
            }

            if (monsterPortraitContainer == null)
            {
                var t = transform.Find("MonsterPortraitContainer");
                if (t != null) monsterPortraitContainer = t.gameObject;
            }
        }

        private void Start()
        {
            // Wire buttons
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            // Bag is removed in BattleScene; ensure any residual reference is hidden
            if (bagButton != null) bagButton.gameObject.SetActive(false);
            if (bagModal != null) bagModal.gameObject.SetActive(false);
            if (victoryReturnButton != null) victoryReturnButton.onClick.AddListener(OnReturnToHallwayClicked);
            if (defeatRetryButton != null) defeatRetryButton.onClick.AddListener(OnRetryClicked);
            if (defeatExitButton != null) defeatExitButton.onClick.AddListener(OnReturnToHallwayClicked);
            if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(OnCloseSettingsClicked);
            if (settingsExitHallwayButton != null) settingsExitHallwayButton.onClick.AddListener(OnReturnToHallwayClicked);

            for (int i = 0; i < artifactSlotButtons.Count; i++)
            {
                int idx = i;
                if (artifactSlotButtons[i] != null)
                {
                    artifactSlotButtons[i].onClick.AddListener(() => OnArtifactSlotClicked(idx));
                }
            }

            for (int i = 0; i < choiceButtons.Count; i++)
            {
                int choiceIdx = i;
                if (choiceButtons[i] != null)
                {
                    choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIdx));
                }
            }

            // Hide modals
            if (victoryModal != null) victoryModal.SetActive(false);
            if (defeatModal != null) defeatModal.SetActive(false);
            if (settingsModal != null) settingsModal.SetActive(false);

            // Initialize Player
            playerCurrentHealth = playerMaxHealth;
            SetupPlayerUI();

            // Initialize Artifact Slots
            SetupArtifactSlots();

            // Initial Dialogue State
            SetInitialDialogue();
        }

        private void SetupPlayerUI()
        {
            string chosenChar = PlayerPrefs.GetString("SelectedCharacter", "Boy");
            bool isGirl = chosenChar == "Girl";
            string pName = isGirl ? "Darla" : "Darrel";

            if (playerNameText != null)
            {
                playerNameText.text = pName;
            }
            if (playerSpeakerText != null)
            {
                playerSpeakerText.text = pName;
            }

            if (playerAvatarImage != null)
            {
                playerAvatarImage.sprite = isGirl ? girlAvatarSprite : boyAvatarSprite;
            }

            UpdatePlayerHearts();
        }

        private void UpdatePlayerHearts()
        {
            for (int i = 0; i < playerHeartImages.Count; i++)
            {
                if (playerHeartImages[i] == null) continue;
                bool isFull = i < playerCurrentHealth;
                playerHeartImages[i].sprite = isFull ? heartFullSprite : heartEmptySprite;
                playerHeartImages[i].color = isFull ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.45f);
            }
        }

        private void SetupArtifactSlots()
        {
            _retrievedCount = 0;
            for (int i = 0; i < artifactSlots.Count; i++)
            {
                var slot = artifactSlots[i];
                if (slot == null) continue;

                slot.isFailed = false;

                // Check if already in inventory
                if (PlayerInventory.HasArtifact(slot.artifactId))
                {
                    slot.isRetrieved = true;
                    _retrievedCount++;
                }

                if (i < artifactSlotImages.Count && artifactSlotImages[i] != null)
                {
                    if (slot.artifactSprite != null)
                    {
                        artifactSlotImages[i].sprite = slot.artifactSprite;
                    }
                    if (slot.isRetrieved)
                    {
                        artifactSlotImages[i].color = new Color(0.4f, 0.4f, 0.4f, 0.7f);
                    }
                    else if (slot.isFailed)
                    {
                        artifactSlotImages[i].color = new Color(0.08f, 0.08f, 0.08f, 1f);
                    }
                    else
                    {
                        artifactSlotImages[i].color = Color.white;
                    }
                }

                if (i < artifactSelectionBorders.Count && artifactSelectionBorders[i] != null)
                {
                    artifactSelectionBorders[i].SetActive(false);
                }

                if (i < artifactCheckmarkIcons.Count && artifactCheckmarkIcons[i] != null)
                {
                    artifactCheckmarkIcons[i].SetActive(slot.isRetrieved);
                }

                if (i < artifactSlotButtons.Count && artifactSlotButtons[i] != null)
                {
                    artifactSlotButtons[i].interactable = !slot.isRetrieved && !slot.isFailed;
                }
            }

            UpdateCounterUI();
        }

        private void UpdateCounterUI()
        {
            if (artifactCounterText != null)
            {
                artifactCounterText.text = $"Artifacts: {_retrievedCount}/{artifactSlots.Count}";
            }
        }

        private GameObject TopDialogueBoxGO
        {
            get
            {
                if (topDialogueBox != null) return topDialogueBox;
                if (questionBodyText != null)
                {
                    Transform p = questionBodyText.transform.parent;
                    while (p != null && p.name != "BattleCanvas")
                    {
                        if (p.name == "TopDialogueBox") { topDialogueBox = p.gameObject; return topDialogueBox; }
                        p = p.parent;
                    }
                }
                var t = transform.Find("TopDialogueBox");
                if (t != null) topDialogueBox = t.gameObject;
                return topDialogueBox;
            }
        }

        private GameObject MonsterPortraitContainerGO
        {
            get
            {
                if (monsterPortraitContainer != null) return monsterPortraitContainer;
                var t = transform.Find("MonsterPortraitContainer");
                if (t != null) monsterPortraitContainer = t.gameObject;
                return monsterPortraitContainer;
            }
        }

        private void SetInitialDialogue()
        {
            _selectedSlotIndex = -1;
            string chosenChar = PlayerPrefs.GetString("SelectedCharacter", "Boy");
            string pName = chosenChar == "Girl" ? "Darla" : "Darrel";

            // Hide question, enemy dialogue, enemy portrait, and choices when entering or choosing
            if (TopDialogueBoxGO != null)
            {
                TopDialogueBoxGO.SetActive(false);
            }
            if (MonsterPortraitContainerGO != null)
            {
                MonsterPortraitContainerGO.SetActive(false);
            }
            if (choicesContainer != null)
            {
                choicesContainer.SetActive(false);
            }

            if (questionSpeakerText != null)
            {
                questionSpeakerText.text = monsterSpeakerName;
            }
            if (questionBodyText != null)
            {
                questionBodyText.text = "You dare enter my realm? Choose an artifact to face my trial!";
            }

            // Keep user dialogue active when entering battle / choosing artifact
            string userPrompt = _retrievedCount > 0 ? "Which artifact should I retrieve next?" : "Which artifact should I retrieve first?";
            if (playerSpeakerText != null)
            {
                playerSpeakerText.text = pName;
            }
            if (playerDialogueText != null)
            {
                playerDialogueText.text = userPrompt;
            }

            if (dialogueSpeakerText != null)
            {
                dialogueSpeakerText.text = pName;
            }
            if (dialogueBodyText != null)
            {
                dialogueBodyText.text = userPrompt;
            }

            // Clear borders
            for (int i = 0; i < artifactSelectionBorders.Count; i++)
            {
                if (artifactSelectionBorders[i] != null) artifactSelectionBorders[i].SetActive(false);
            }
        }

        public void OnArtifactSlotClicked(int index)
        {
            if (_isBusy) return;
            if (index < 0 || index >= artifactSlots.Count) return;

            var slot = artifactSlots[index];
            if (slot.isRetrieved || slot.isFailed) return;

            _selectedSlotIndex = index;

            // Reveal question, enemy dialogue, enemy portrait, and choices
            if (TopDialogueBoxGO != null)
            {
                TopDialogueBoxGO.SetActive(true);
            }
            if (MonsterPortraitContainerGO != null)
            {
                MonsterPortraitContainerGO.SetActive(true);
            }
            if (choicesContainer != null)
            {
                choicesContainer.SetActive(true);
            }

            // Highlight border
            for (int i = 0; i < artifactSelectionBorders.Count; i++)
            {
                if (artifactSelectionBorders[i] != null)
                {
                    artifactSelectionBorders[i].SetActive(i == index);
                }
            }

            // Display question in Top Box
            if (questionSpeakerText != null)
            {
                questionSpeakerText.text = $"[QUIZ] {slot.artifactName}";
            }
            if (questionBodyText != null)
            {
                questionBodyText.text = slot.questionText;
            }

            // Player reaction in Bottom Player Box
            string chosenChar = PlayerPrefs.GetString("SelectedCharacter", "Boy");
            string pName = chosenChar == "Girl" ? "Darla" : "Darrel";
            if (playerSpeakerText != null)
            {
                playerSpeakerText.text = pName;
            }
            if (playerDialogueText != null)
            {
                playerDialogueText.text = "Ano nga ba ulit sagot dito....";
            }

            // Legacy fallbacks
            if (dialogueSpeakerText != null)
            {
                dialogueSpeakerText.text = $"[QUIZ] {slot.artifactName}";
            }
            if (dialogueBodyText != null)
            {
                dialogueBodyText.text = slot.questionText;
            }

            // Setup choice buttons
            if (choicesContainer != null)
            {
                choicesContainer.SetActive(true);
            }

            for (int c = 0; c < choiceButtons.Count; c++)
            {
                if (choiceButtons[c] == null) continue;

                if (c < slot.choices.Length && !string.IsNullOrEmpty(slot.choices[c]))
                {
                    choiceButtons[c].gameObject.SetActive(true);
                    choiceButtons[c].interactable = true;
                    if (c < choiceButtonTexts.Count && choiceButtonTexts[c] != null)
                    {
                        choiceButtonTexts[c].text = slot.choices[c];
                    }
                }
                else
                {
                    choiceButtons[c].gameObject.SetActive(false);
                }
            }
        }

        private void OnChoiceSelected(int choiceIndex)
        {
            if (_isBusy || _selectedSlotIndex < 0 || _selectedSlotIndex >= artifactSlots.Count) return;

            var slot = artifactSlots[_selectedSlotIndex];
            _isBusy = true;

            // Disable buttons during evaluation
            for (int i = 0; i < choiceButtons.Count; i++)
            {
                if (choiceButtons[i] != null) choiceButtons[i].interactable = false;
            }

            bool isCorrect = (choiceIndex == slot.correctChoiceIndex);

            if (isCorrect)
            {
                StartCoroutine(CorrectAnswerRoutine(slot));
            }
            else
            {
                StartCoroutine(WrongAnswerRoutine(slot));
            }
        }

        private IEnumerator CorrectAnswerRoutine(ArtifactBattleSlot slot)
        {
            if (questionSpeakerText != null) questionSpeakerText.text = $"[QUIZ] {slot.artifactName}";
            if (questionBodyText != null) questionBodyText.text = "Argh! How did you know that?!";

            string successMsg = $"<color=#1E7E34><b>CORRECT!</b></color> You recovered the {slot.artifactName}! You dealt damage to the monster!";
            if (playerDialogueText != null) playerDialogueText.text = successMsg;
            if (dialogueBodyText != null) dialogueBodyText.text = successMsg;

            // 1. Monster takes damage with animation
            if (monsterController != null)
            {
                bool animDone = false;
                monsterController.TakeDamage(2, () => animDone = true);
                while (!animDone) yield return null;
            }

            // 2. Mark artifact retrieved & add to bag
            slot.isRetrieved = true;
            _retrievedCount++;
            PlayerInventory.AddArtifact(slot.artifactId, slot.artifactName, slot.artifactSprite, slot.artifactDescription);

            if (roomNumber == 1)
            {
                bool justRetrieved = GameProgression.RetrieveRoom1Artifact();
                if (justRetrieved)
                {
                    NotificationBannerUI.Show(
                        "ARTIFACT RECOVERED!",
                        "You recovered an artifact! Bring it back inside Room 1 in the Real World to open Room 2!",
                        5.5f,
                        new Color(0.2f, 0.9f, 0.4f, 1f),
                        "!"
                    );
                }
            }

            // Update slot visuals
            if (_selectedSlotIndex < artifactSlotImages.Count && artifactSlotImages[_selectedSlotIndex] != null)
            {
                artifactSlotImages[_selectedSlotIndex].color = new Color(0.4f, 0.4f, 0.4f, 0.7f);
            }
            if (_selectedSlotIndex < artifactCheckmarkIcons.Count && artifactCheckmarkIcons[_selectedSlotIndex] != null)
            {
                artifactCheckmarkIcons[_selectedSlotIndex].SetActive(true);
            }
            if (_selectedSlotIndex < artifactSlotButtons.Count && artifactSlotButtons[_selectedSlotIndex] != null)
            {
                artifactSlotButtons[_selectedSlotIndex].interactable = false;
            }

            UpdateCounterUI();

            yield return new WaitForSeconds(1.2f);

            CheckBattleProgress();
        }

        private IEnumerator WrongAnswerRoutine(ArtifactBattleSlot slot)
        {
            if (questionSpeakerText != null) questionSpeakerText.text = $"[QUIZ] {slot.artifactName}";
            if (questionBodyText != null) questionBodyText.text = "Foolish mortal! Suffer my wrath!";

            string failMsg = $"<color=#C82333><b>INCORRECT!</b></color> The monster counterattacked! You lost 1 HP and failed to retrieve {slot.artifactName}!";
            if (playerDialogueText != null) playerDialogueText.text = failMsg;
            if (dialogueBodyText != null) dialogueBodyText.text = failMsg;

            // 1. Mark artifact as failed (NOT retrieved, player does NOT get it)
            slot.isFailed = true;

            // Black out the artifact image
            if (_selectedSlotIndex < artifactSlotImages.Count && artifactSlotImages[_selectedSlotIndex] != null)
            {
                artifactSlotImages[_selectedSlotIndex].color = new Color(0.08f, 0.08f, 0.08f, 1f);
            }
            if (_selectedSlotIndex < artifactSlotButtons.Count && artifactSlotButtons[_selectedSlotIndex] != null)
            {
                artifactSlotButtons[_selectedSlotIndex].interactable = false;
            }
            if (_selectedSlotIndex < artifactSelectionBorders.Count && artifactSelectionBorders[_selectedSlotIndex] != null)
            {
                artifactSelectionBorders[_selectedSlotIndex].SetActive(false);
            }

            // 2. Player takes damage
            playerCurrentHealth = Mathf.Max(0, playerCurrentHealth - 1);
            UpdatePlayerHearts();

            // 3. Shake player avatar portrait
            if (playerPortraitTransform != null)
            {
                yield return StartCoroutine(PlayerDamageRoutine());
            }

            yield return new WaitForSeconds(1.0f);

            // Check if player defeat
            if (playerCurrentHealth <= 0)
            {
                TriggerDefeat();
            }
            else
            {
                // Battle continues even if wrong!
                CheckBattleProgress();
            }
        }

        private void CheckBattleProgress()
        {
            int attemptedCount = 0;
            for (int i = 0; i < artifactSlots.Count; i++)
            {
                if (artifactSlots[i] != null && (artifactSlots[i].isRetrieved || artifactSlots[i].isFailed))
                {
                    attemptedCount++;
                }
            }

            bool monsterDefeated = monsterController != null && monsterController.IsDefeated;
            bool allAttempted = attemptedCount >= artifactSlots.Count;

            if (monsterDefeated || allAttempted)
            {
                TriggerBattleFinished();
            }
            else
            {
                SetInitialDialogue();
                _isBusy = false;
            }
        }

        private IEnumerator PlayerDamageRoutine()
        {
            if (playerPortraitTransform == null) yield break;

            Vector3 origPos = playerPortraitTransform.localPosition;
            float dur = 0.4f;
            float el = 0f;

            while (el < dur)
            {
                el += Time.deltaTime;
                float mag = Mathf.Lerp(12f, 0f, el / dur);
                float ox = Mathf.Sin(el * 60f) * mag;
                playerPortraitTransform.localPosition = origPos + new Vector3(ox, 0f, 0f);
                yield return null;
            }

            playerPortraitTransform.localPosition = origPos;
        }

        private void TriggerBattleFinished()
        {
            _isBusy = true;
            if (TopDialogueBoxGO != null) TopDialogueBoxGO.SetActive(false);
            if (MonsterPortraitContainerGO != null) MonsterPortraitContainerGO.SetActive(false);
            if (choicesContainer != null) choicesContainer.SetActive(false);

            if (roomNumber == 1)
            {
                GameProgression.CompleteRoom1Battle();
                if (_retrievedCount > 0)
                {
                    GameProgression.RetrieveRoom1Artifact();
                }
            }

            if (_retrievedCount > 0 || (monsterController != null && monsterController.IsDefeated))
            {
                if (victoryModal != null)
                {
                    var bodyText = victoryModal.transform.Find("Window/Body")?.GetComponent<TMP_Text>();
                    if (bodyText != null)
                    {
                        bodyText.text = $"You completed the battle and retrieved {_retrievedCount} of {artifactSlots.Count} artifact(s)!\nTake the portal back and bring them inside Room 1 in the Real World to open Alterworld Room 2!";
                    }
                    victoryModal.SetActive(true);
                }
            }
            else
            {
                // All attempted were wrong (0 retrieved)
                if (defeatModal != null)
                {
                    var titleText = defeatModal.transform.Find("Window/Title")?.GetComponent<TMP_Text>();
                    if (titleText != null) titleText.text = "TRIAL COMPLETED";
                    var bodyText = defeatModal.transform.Find("Window/Body")?.GetComponent<TMP_Text>();
                    if (bodyText != null)
                    {
                        bodyText.text = "You were unable to recover any artifacts from this room!\nReturning to hallway...";
                    }
                    defeatModal.SetActive(true);
                }
            }
            Debug.Log($"[QuizBattleManager] Battle finished! Retrieved: {_retrievedCount}/{artifactSlots.Count}");
        }

        private void TriggerVictory()
        {
            TriggerBattleFinished();
        }

        private void TriggerDefeat()
        {
            _isBusy = true;
            if (TopDialogueBoxGO != null) TopDialogueBoxGO.SetActive(false);
            if (MonsterPortraitContainerGO != null) MonsterPortraitContainerGO.SetActive(false);
            if (choicesContainer != null) choicesContainer.SetActive(false);

            if (roomNumber == 1)
            {
                GameProgression.CompleteRoom1Battle();
                if (_retrievedCount > 0)
                {
                    GameProgression.RetrieveRoom1Artifact();
                }
            }

            if (defeatModal != null)
            {
                var titleText = defeatModal.transform.Find("Window/Title")?.GetComponent<TMP_Text>();
                if (titleText != null) titleText.text = "DEFEATED";
                var bodyText = defeatModal.transform.Find("Window/Body")?.GetComponent<TMP_Text>();
                if (bodyText != null)
                {
                    if (_retrievedCount > 0)
                    {
                        bodyText.text = $"You fell in battle, but managed to keep {_retrievedCount} recovered artifact(s)!\nRoom 2 in the Alterworld is open.";
                    }
                    else
                    {
                        bodyText.text = "You were overwhelmed by the monster!\nReturning to hallway...";
                    }
                }
                defeatModal.SetActive(true);
            }
            Debug.Log("[QuizBattleManager] Battle LOST! Player health depleted!");
        }

        public void OnSettingsClicked()
        {
            if (settingsModal != null) settingsModal.SetActive(true);
        }

        public void OnCloseSettingsClicked()
        {
            if (settingsModal != null) settingsModal.SetActive(false);
        }

        public void OnRetryClicked()
        {
            OnReturnToHallwayClicked();
        }

        public void OnReturnToHallwayClicked()
        {
            if (roomNumber == 1)
            {
                GameProgression.CompleteRoom1Battle();
                if (_retrievedCount > 0)
                {
                    GameProgression.RetrieveRoom1Artifact();
                }
            }
            RoomTransitionManager.ExitToLobby();
        }
    }
}
