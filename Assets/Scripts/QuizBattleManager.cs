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
        [SerializeField] private GameObject bottomDialogueBox;
        [SerializeField] private GameObject playerPortraitContainer;
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
        private int _currentCorrectChoiceButtonIndex = -1;

        private class CachedChoiceData
        {
            public List<string> optionTexts = new List<string>();
            public int correctButtonIndex = -1;
        }

        private readonly Dictionary<int, CachedChoiceData> _cachedSlotChoices = new Dictionary<int, CachedChoiceData>();

        private static string CleanChoiceText(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            string trimmed = raw.Trim();
            if (trimmed.Length >= 2 && char.IsLetter(trimmed[0]) && (trimmed[1] == '.' || trimmed[1] == ')' || trimmed[1] == ':'))
            {
                trimmed = trimmed.Substring(2).TrimStart();
            }
            return trimmed;
        }

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
                if (t == null)
                {
                    var canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
                    if (canvas != null) t = canvas.transform.Find("MonsterPortraitContainer");
                }
                if (t != null) monsterPortraitContainer = t.gameObject;
            }

            if (bottomDialogueBox == null)
            {
                var canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    var t = canvas.transform.Find("BottomDialogueBox");
                    if (t != null) bottomDialogueBox = t.gameObject;
                }
            }

            if (playerPortraitContainer == null)
            {
                var canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    var t = canvas.transform.Find("PlayerPortraitContainer");
                    if (t != null) playerPortraitContainer = t.gameObject;
                }
            }

            EnsureMonsterPortrait(false);
        }

        private void Start()
        {
            _cachedSlotChoices.Clear();

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

                    // Remove A, B, C, D badge and expand text area
                    var badge = choiceButtons[i].transform.Find("Badge");
                    if (badge != null)
                    {
                        badge.gameObject.SetActive(false);
                    }

                    var textChild = choiceButtons[i].transform.Find("Text");
                    if (textChild != null)
                    {
                        var rt = textChild.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.anchorMin = Vector2.zero;
                            rt.anchorMax = Vector2.one;
                            rt.offsetMin = new Vector2(24f, 0f);
                            rt.offsetMax = new Vector2(-24f, 0f);
                        }
                        var tmp = textChild.GetComponent<TMP_Text>();
                        if (tmp != null)
                        {
                            tmp.alignment = TextAlignmentOptions.MidlineLeft;
                        }
                    }
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

            // Initial Battle Notification
            NotificationBannerUI.Show(
                $"ALTERWORLD BATTLE {roomNumber}",
                $"Trial of Room {roomNumber}: Select a corrupted artifact below to challenge the monster!",
                5.0f,
                new Color(0.24f, 0.14f, 0.07f, 1f),
                "⚔"
            );
        }

        private string GetPlayerCharacterName()
        {
            string chosenChar = PlayerPrefs.GetString("SelectedCharacter", "Boy");
            string defaultName = chosenChar == "Girl" ? "Darla" : "Darrel";
            return PlayerPrefs.GetString("CustomCharacterName", defaultName);
        }

        private void SetupPlayerUI()
        {
            string chosenChar = PlayerPrefs.GetString("SelectedCharacter", "Boy");
            bool isGirl = chosenChar == "Girl";
            string pName = GetPlayerCharacterName();

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

                // Check if already obtained (in bag or restored on pedestal in real world)
                if (GameProgression.IsArtifactObtained(roomNumber, slot.artifactId, slot.artifactName))
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

            // Scale monster health to remaining unobtained artifacts so retry battles are winnable and feel great
            int remainingUnobtained = artifactSlots.Count - _retrievedCount;
            if (monsterController != null && remainingUnobtained > 0)
            {
                monsterController.SetMaxAndCurrentHealth(remainingUnobtained);
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
                if (t == null)
                {
                    var canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
                    if (canvas != null) t = canvas.transform.Find("MonsterPortraitContainer");
                }
                if (t == null)
                {
                    var allGo = Resources.FindObjectsOfTypeAll<GameObject>();
                    for (int i = 0; i < allGo.Length; i++)
                    {
                        if (allGo[i].name == "MonsterPortraitContainer")
                        {
                            t = allGo[i].transform;
                            break;
                        }
                    }
                }
                if (t != null) monsterPortraitContainer = t.gameObject;
                return monsterPortraitContainer;
            }
        }

        public void EnsureMonsterPortrait(bool setActive = true)
        {
            var go = MonsterPortraitContainerGO;
            if (go != null)
            {
                // Ensure Frame and Avatar are active and have a valid sprite
                var avatarTr = go.transform.Find("Frame/Avatar") ?? go.transform.Find("Avatar");
                if (avatarTr != null)
                {
                    avatarTr.gameObject.SetActive(true);
                    var avatarImg = avatarTr.GetComponent<Image>();
                    if (avatarImg != null)
                    {
                        avatarImg.enabled = true;
                        if (avatarImg.color.a < 0.1f) avatarImg.color = Color.white;

                        if (avatarImg.sprite == null && monsterController != null)
                        {
                            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
                            var mSpr = monsterController.GetType().GetField("monsterSprite", flags)?.GetValue(monsterController) as Sprite;
                            if (mSpr != null) avatarImg.sprite = mSpr;
                        }

                        if (avatarImg.sprite == null)
                        {
                            string spriteName = roomNumber <= 1 ? "Avatar_Monster_0" : $"Avatar_Enemy_Room{roomNumber}";
                            var spr = Resources.Load<Sprite>(spriteName);
#if UNITY_EDITOR
                            if (spr == null) spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{spriteName}.png");
                            if (spr == null) spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(roomNumber <= 1 ? "Assets/Sprites/Enemy_Room1.png" : $"Assets/Sprites/Enemy_Room{roomNumber}.png");
#endif
                            if (spr != null) avatarImg.sprite = spr;
                        }
                    }
                }

                var frameTr = go.transform.Find("Frame");
                if (frameTr != null) frameTr.gameObject.SetActive(true);

                var heartsTr = go.transform.Find("MonsterHearts");
                if (heartsTr != null) heartsTr.gameObject.SetActive(true);

                go.SetActive(setActive);
            }
        }

        private void SetInitialDialogue()
        {
            _selectedSlotIndex = -1;
            string pName = GetPlayerCharacterName();

            // Do NOT show enemy icon and health immediately; they only appear when the user clicks an artifact
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

            // If the player clicks the currently selected active slot again, do not re-run or re-shuffle anything!
            if (_selectedSlotIndex == index && choicesContainer != null && choicesContainer.activeSelf)
            {
                return;
            }

            _selectedSlotIndex = index;

            // Reveal question, enemy dialogue, enemy portrait, and choices when an artifact is clicked
            if (TopDialogueBoxGO != null)
            {
                TopDialogueBoxGO.SetActive(true);
            }
            EnsureMonsterPortrait(true);
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
            string pName = GetPlayerCharacterName();
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

            // Setup choice buttons with randomized positions (shuffled once and persistent per battle session)
            if (choicesContainer != null)
            {
                choicesContainer.SetActive(true);
            }

            if (!_cachedSlotChoices.TryGetValue(index, out var cachedData))
            {
                var options = new List<(string text, bool isCorrect)>();
                for (int c = 0; c < slot.choices.Length; c++)
                {
                    if (!string.IsNullOrEmpty(slot.choices[c]))
                    {
                        string cleaned = CleanChoiceText(slot.choices[c]);
                        bool isCorrect = (c == slot.correctChoiceIndex);
                        options.Add((cleaned, isCorrect));
                    }
                }

                // Shuffle choices once so correct answer position is randomized for this battle session
                for (int i = options.Count - 1; i > 0; i--)
                {
                    int r = UnityEngine.Random.Range(0, i + 1);
                    var temp = options[i];
                    options[i] = options[r];
                    options[r] = temp;
                }

                cachedData = new CachedChoiceData();
                for (int c = 0; c < options.Count; c++)
                {
                    cachedData.optionTexts.Add(options[c].text);
                    if (options[c].isCorrect)
                    {
                        cachedData.correctButtonIndex = c;
                    }
                }
                _cachedSlotChoices[index] = cachedData;
            }

            _currentCorrectChoiceButtonIndex = cachedData.correctButtonIndex;

            for (int c = 0; c < choiceButtons.Count; c++)
            {
                if (choiceButtons[c] == null) continue;

                if (c < cachedData.optionTexts.Count)
                {
                    choiceButtons[c].gameObject.SetActive(true);
                    choiceButtons[c].interactable = true;
                    if (c < choiceButtonTexts.Count && choiceButtonTexts[c] != null)
                    {
                        choiceButtonTexts[c].text = cachedData.optionTexts[c];
                    }

                    // Hide A, B, C, D badge so it's just the text choices only
                    var badge = choiceButtons[c].transform.Find("Badge");
                    if (badge != null && badge.gameObject.activeSelf)
                    {
                        badge.gameObject.SetActive(false);
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

            bool isCorrect = (_currentCorrectChoiceButtonIndex >= 0)
                ? (choiceIndex == _currentCorrectChoiceButtonIndex)
                : (choiceIndex == slot.correctChoiceIndex);

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
                monsterController.TakeDamage(1, () => animDone = true);
                while (!animDone) yield return null;
            }

            // 2. Mark artifact retrieved & add to bag
            slot.isRetrieved = true;
            _retrievedCount++;
            PlayerInventory.AddArtifact(slot.artifactId, slot.artifactName, slot.artifactSprite, slot.artifactDescription);

            GameProgression.RetrieveRoomArtifact(roomNumber);
            GameProgression.SetRoomRetrievedCount(roomNumber, _retrievedCount);

            if (_retrievedCount >= artifactSlots.Count)
            {
                NotificationBannerUI.Show(
                    "ALL ARTIFACTS RECOVERED!",
                    $"You recovered '{slot.artifactName}'! All {artifactSlots.Count} artifacts saved in Room {roomNumber}!",
                    5.0f,
                    new Color(0.18f, 0.40f, 0.16f, 1f),
                    "★"
                );
            }
            else
            {
                NotificationBannerUI.Show(
                    "ARTIFACT RECOVERED!",
                    $"You recovered the {slot.artifactName}! ({_retrievedCount}/{artifactSlots.Count} retrieved)",
                    4.5f,
                    new Color(0.18f, 0.40f, 0.16f, 1f),
                    "★"
                );
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

            NotificationBannerUI.Show(
                "INCORRECT ANSWER!",
                $"Monster counterattacked! You took 1 damage and failed to retrieve '{slot.artifactName}'.",
                3.5f,
                new Color(0.55f, 0.12f, 0.10f, 1f),
                "!"
            );

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
            if (choicesContainer != null) choicesContainer.SetActive(false);

            if (_retrievedCount > 0)
            {
                // PASSED! Player retrieved at least 1 artifact
                GameProgression.CompleteRoomBattle(roomNumber);
                GameProgression.RetrieveRoomArtifact(roomNumber);
                GameProgression.SetRoomRetrievedCount(roomNumber, _retrievedCount);

                NotificationBannerUI.Show(
                    "BATTLE VICTORY!",
                    $"Retrieved {_retrievedCount}/{artifactSlots.Count} artifacts! Bring them to Room {roomNumber} in the Real World.",
                    5.5f,
                    new Color(0.18f, 0.40f, 0.16f, 1f),
                    "★"
                );

                if (victoryModal != null)
                {
                    var bodyText = victoryModal.transform.Find("Window/Body")?.GetComponent<TMP_Text>();
                    if (bodyText != null)
                    {
                        string nextUnlock = (roomNumber < 12)
                            ? $"Take the portal back, enter Room {roomNumber} in the Real World, and place your recovered artifact(s) on the pedestals to open Room {roomNumber + 1}!"
                            : $"Take the portal back, enter Room {roomNumber} in the Real World, and place your recovered artifact(s) on the pedestals!";
                        bodyText.text = $"You completed the battle and retrieved {_retrievedCount} of {artifactSlots.Count} artifact(s)!\n{nextUnlock}";
                    }
                    victoryModal.transform.SetAsLastSibling();
                    victoryModal.SetActive(true);
                }
            }
            else
            {
                // FAILED: 0 artifacts retrieved! Must retake the battle to progress
                GameProgression.SetRoomRetrievedCount(roomNumber, 0);

                if (defeatModal != null)
                {
                    var titleText = defeatModal.transform.Find("Window/Title")?.GetComponent<TMP_Text>();
                    if (titleText != null) titleText.text = "TRIAL FAILED - 0 ARTIFACTS";
                    var bodyText = defeatModal.transform.Find("Window/Body")?.GetComponent<TMP_Text>();
                    if (bodyText != null)
                    {
                        bodyText.text = "You scored 0 and recovered no artifacts!\nYou must retake the battle to retrieve at least 1 artifact and progress.";
                    }
                    var retryBtnText = defeatRetryButton != null ? defeatRetryButton.GetComponentInChildren<TMP_Text>() : null;
                    if (retryBtnText != null) retryBtnText.text = "RETAKE BATTLE";
                    defeatModal.transform.SetAsLastSibling();
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
            if (choicesContainer != null) choicesContainer.SetActive(false);

            if (_retrievedCount > 0)
            {
                // Managed to retrieve at least 1 artifact before defeat -> can progress!
                GameProgression.CompleteRoomBattle(roomNumber);
                GameProgression.RetrieveRoomArtifact(roomNumber);
                GameProgression.SetRoomRetrievedCount(roomNumber, _retrievedCount);

                NotificationBannerUI.Show(
                    "TRIAL ENDED",
                    $"You fell in battle, but kept {_retrievedCount} recovered artifact(s)!",
                    5.0f,
                    new Color(0.24f, 0.14f, 0.07f, 1f),
                    "!"
                );

                if (defeatModal != null)
                {
                    var titleText = defeatModal.transform.Find("Window/Title")?.GetComponent<TMP_Text>();
                    if (titleText != null) titleText.text = "TRIAL ENDED";
                    var bodyText = defeatModal.transform.Find("Window/Body")?.GetComponent<TMP_Text>();
                    if (bodyText != null)
                    {
                        bodyText.text = $"You fell in battle, but kept {_retrievedCount} recovered artifact(s)!\nBring them back inside Room {roomNumber} in the Real World to open Room {roomNumber + 1}.";
                    }
                    defeatModal.transform.SetAsLastSibling();
                    defeatModal.SetActive(true);
                }
            }
            else
            {
                // FAILED: 0 artifacts retrieved! Must retake the battle to progress
                GameProgression.SetRoomRetrievedCount(roomNumber, 0);

                NotificationBannerUI.Show(
                    "TRIAL FAILED",
                    "You were defeated and recovered 0 artifacts! Retake the battle to progress.",
                    5.0f,
                    new Color(0.55f, 0.12f, 0.10f, 1f),
                    "!"
                );

                if (defeatModal != null)
                {
                    var titleText = defeatModal.transform.Find("Window/Title")?.GetComponent<TMP_Text>();
                    if (titleText != null) titleText.text = "TRIAL FAILED - 0 ARTIFACTS";
                    var bodyText = defeatModal.transform.Find("Window/Body")?.GetComponent<TMP_Text>();
                    if (bodyText != null)
                    {
                        bodyText.text = "You were defeated and recovered 0 artifacts!\nYou must retake the battle to retrieve at least 1 artifact and progress.";
                    }
                    var retryBtnText = defeatRetryButton != null ? defeatRetryButton.GetComponentInChildren<TMP_Text>() : null;
                    if (retryBtnText != null) retryBtnText.text = "RETAKE BATTLE";
                    defeatModal.transform.SetAsLastSibling();
                    defeatModal.SetActive(true);
                }
            }
            Debug.Log("[QuizBattleManager] Battle LOST! Player health depleted!");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settingsModal != null && settingsModal.activeSelf)
                {
                    OnCloseSettingsClicked();
                }
                else if ((victoryModal == null || !victoryModal.activeSelf) &&
                         (defeatModal == null || !defeatModal.activeSelf))
                {
                    OnSettingsClicked();
                }
            }
        }

        private bool _wasChoicesActiveBeforePause = false;
        private bool _wasTopDialogueActiveBeforePause = false;
        private bool _wasBottomDialogueActiveBeforePause = false;
        private bool _wasPlayerPortraitActiveBeforePause = false;
        private bool _wasMonsterPortraitActiveBeforePause = false;

        public void OnSettingsClicked()
        {
            if (settingsModal != null)
            {
                if (bottomDialogueBox == null)
                {
                    var canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
                    if (canvas != null) bottomDialogueBox = canvas.transform.Find("BottomDialogueBox")?.gameObject;
                }
                if (playerPortraitContainer == null)
                {
                    var canvas = GetComponentInParent<Canvas>() ?? FindFirstObjectByType<Canvas>();
                    if (canvas != null) playerPortraitContainer = canvas.transform.Find("PlayerPortraitContainer")?.gameObject;
                }

                _wasChoicesActiveBeforePause = choicesContainer != null && choicesContainer.activeSelf;
                _wasTopDialogueActiveBeforePause = TopDialogueBoxGO != null && TopDialogueBoxGO.activeSelf;
                _wasMonsterPortraitActiveBeforePause = monsterPortraitContainer != null && monsterPortraitContainer.activeSelf;
                _wasBottomDialogueActiveBeforePause = bottomDialogueBox != null && bottomDialogueBox.activeSelf;
                _wasPlayerPortraitActiveBeforePause = playerPortraitContainer != null && playerPortraitContainer.activeSelf;

                if (choicesContainer != null) choicesContainer.SetActive(false);
                if (TopDialogueBoxGO != null) TopDialogueBoxGO.SetActive(false);
                if (monsterPortraitContainer != null) monsterPortraitContainer.SetActive(false);
                if (bottomDialogueBox != null) bottomDialogueBox.SetActive(false);
                if (playerPortraitContainer != null) playerPortraitContainer.SetActive(false);

                settingsModal.transform.SetAsLastSibling();
                settingsModal.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        public void OnCloseSettingsClicked()
        {
            if (settingsModal != null)
            {
                settingsModal.SetActive(false);

                if (choicesContainer != null && _wasChoicesActiveBeforePause) choicesContainer.SetActive(true);
                if (TopDialogueBoxGO != null && _wasTopDialogueActiveBeforePause) TopDialogueBoxGO.SetActive(true);
                if (monsterPortraitContainer != null && _wasMonsterPortraitActiveBeforePause) monsterPortraitContainer.SetActive(true);
                if (bottomDialogueBox != null && _wasBottomDialogueActiveBeforePause) bottomDialogueBox.SetActive(true);
                if (playerPortraitContainer != null && _wasPlayerPortraitActiveBeforePause) playerPortraitContainer.SetActive(true);

                Time.timeScale = 1f;
            }
        }

        public void OnRetryClicked()
        {
            Time.timeScale = 1f;
            // Retake the battle fresh
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void OnReturnToHallwayClicked()
        {
            Time.timeScale = 1f;
            if (_retrievedCount > 0)
            {
                GameProgression.CompleteRoomBattle(roomNumber);
                GameProgression.RetrieveRoomArtifact(roomNumber);
                GameProgression.SetRoomRetrievedCount(roomNumber, _retrievedCount);
            }
            // If _retrievedCount == 0, room battle is NOT completed, so player can re-enter to retake it!
            RoomTransitionManager.ExitToLobby();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
