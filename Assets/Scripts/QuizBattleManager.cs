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
        [SerializeField] private string monsterSpeakerName = "Aswang";

        public string MonsterSpeakerName
        {
            get => !string.IsNullOrWhiteSpace(monsterSpeakerName) ? monsterSpeakerName : "Monster";
            set => monsterSpeakerName = value;
        }

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

        [Header("Pause / Settings Sprites")]
        [SerializeField] private Sprite pauseBtnResumeNorm;
        [SerializeField] private Sprite pauseBtnResumeHov;
        [SerializeField] private Sprite pauseBtnExitNorm;
        [SerializeField] private Sprite pauseBtnExitHov;
        [SerializeField] private Sprite pauseBtnRetreatNorm;
        [SerializeField] private Sprite pauseBtnRetreatHov;
        [SerializeField] private Sprite pauseBtnFinalizeNorm;
        [SerializeField] private Sprite pauseBtnFinalizeHov;
        [SerializeField] private Sprite pauseBtnKeepFightingNorm;
        [SerializeField] private Sprite pauseBtnKeepFightingHov;
        [SerializeField] private Sprite pauseConfirmFrame;
        [SerializeField] private Sprite pauseBtnConfirmNorm;
        [SerializeField] private Sprite pauseBtnConfirmHov;
        [SerializeField] private Sprite pauseBtnCancelNorm;
        [SerializeField] private Sprite pauseBtnCancelHov;

        private GameObject _confirmPauseGroup;
        private TMP_Text _pauseTitleText;
        private TMP_Text _pauseStatusText;
        private TMP_Text _confirmTitleText;
        private TMP_Text _confirmMessageText;
        private Button _confirmExitButton;
        private Button _confirmCancelButton;

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
            GameProgression.MarkRoomBattleAttempted(roomNumber);
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
            if (settingsExitHallwayButton != null) settingsExitHallwayButton.onClick.AddListener(OnPauseExitClicked);

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
                questionSpeakerText.text = MonsterSpeakerName;
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
                questionSpeakerText.text = MonsterSpeakerName;
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
                playerDialogueText.text = "What was the answer to this again...?";
            }

            // Legacy fallbacks
            if (dialogueSpeakerText != null)
            {
                dialogueSpeakerText.text = MonsterSpeakerName;
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
            if (questionSpeakerText != null) questionSpeakerText.text = MonsterSpeakerName;
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
            if (questionSpeakerText != null) questionSpeakerText.text = MonsterSpeakerName;
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
                    ApplyDefeatModalTheme(false);
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
                    ApplyDefeatModalTheme(true);
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
                    ApplyDefeatModalTheme(false);
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

        private void ApplyDefeatModalTheme(bool isTrialEnded)
        {
            if (defeatModal == null) return;
            var windowImg = defeatModal.transform.Find("Window")?.GetComponent<UnityEngine.UI.Image>();
            string framePath = isTrialEnded ? "UI/TrialEndedModal_Frame" : "UI/DefeatModal_Frame";
            string normPath = isTrialEnded ? "UI/Btn_TrialEnded_Return_Normal" : "UI/Btn_ReturnToHallway_Normal";
            string pressPath = isTrialEnded ? "UI/Btn_TrialEnded_Return_Pressed" : "UI/Btn_ReturnToHallway_Pressed";

            var frameSprite = Resources.Load<Sprite>(framePath);
            if (windowImg != null && frameSprite != null) windowImg.sprite = frameSprite;

            if (defeatExitButton != null)
            {
                var btnImg = defeatExitButton.GetComponent<UnityEngine.UI.Image>();
                var normSprite = Resources.Load<Sprite>(normPath);
                var pressSprite = Resources.Load<Sprite>(pressPath);
                if (btnImg != null && normSprite != null) btnImg.sprite = normSprite;

                defeatExitButton.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;
                var ss = new UnityEngine.UI.SpriteState();
                ss.highlightedSprite = normSprite;
                ss.pressedSprite = pressSprite;
                ss.selectedSprite = normSprite;
                defeatExitButton.spriteState = ss;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settingsModal != null && settingsModal.activeSelf)
                {
                    if (_confirmPauseGroup != null && _confirmPauseGroup.activeSelf)
                    {
                        OnCancelExitClicked();
                    }
                    else
                    {
                        OnCloseSettingsClicked();
                    }
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

                EnsurePauseUIReferences();
                UpdatePauseUI();

                if (_confirmPauseGroup != null) _confirmPauseGroup.SetActive(false);
                SetMainPauseViewActive(true);

                settingsModal.transform.SetAsLastSibling();
                settingsModal.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        public void OnCloseSettingsClicked()
        {
            if (settingsModal != null)
            {
                if (_confirmPauseGroup != null) _confirmPauseGroup.SetActive(false);
                SetMainPauseViewActive(true);

                settingsModal.SetActive(false);

                if (choicesContainer != null && _wasChoicesActiveBeforePause) choicesContainer.SetActive(true);
                if (TopDialogueBoxGO != null && _wasTopDialogueActiveBeforePause) TopDialogueBoxGO.SetActive(true);
                if (monsterPortraitContainer != null && _wasMonsterPortraitActiveBeforePause) monsterPortraitContainer.SetActive(true);
                if (bottomDialogueBox != null && _wasBottomDialogueActiveBeforePause) bottomDialogueBox.SetActive(true);
                if (playerPortraitContainer != null && _wasPlayerPortraitActiveBeforePause) playerPortraitContainer.SetActive(true);

                Time.timeScale = 1f;
            }
        }

        public void OnPauseExitClicked()
        {
            EnsurePauseUIReferences();
            bool hasRetrieved = _retrievedCount > 0;

            if (_confirmMessageText != null)
            {
                _confirmMessageText.text = hasRetrieved
                    ? $"You have recovered <color=#7A1B0C><b>{_retrievedCount} of 5 artifacts</b></color>.\n\nRetreating now will <b>conclude</b> this trial and preserve your relics.\n\nAre you sure you want to retreat to the hallway?"
                    : "You have not recovered any artifacts yet.\n\nRetreating now will return you to the hallway without saving progress.\n\nAre you sure you want to retreat to the hallway?";
            }

            SetMainPauseViewActive(false);
            if (_confirmPauseGroup != null) _confirmPauseGroup.SetActive(true);
        }

        public void OnCancelExitClicked()
        {
            if (_confirmPauseGroup != null) _confirmPauseGroup.SetActive(false);
            SetMainPauseViewActive(true);
        }

        private void SetMainPauseViewActive(bool active)
        {
            if (settingsModal == null) return;
            var windowTr = settingsModal.transform.Find("Window");
            if (windowTr != null)
            {
                windowTr.gameObject.SetActive(active);
            }
        }

        private void UpdatePauseUI()
        {
            EnsurePauseUIReferences();
            bool hasRetrieved = _retrievedCount > 0;

            if (_pauseStatusText != null)
            {
                _pauseStatusText.text = hasRetrieved
                    ? $"<color=#7A1B0C><b>Retrieved: {_retrievedCount} / 5 Artifacts</b></color>"
                    : "<color=#3A2212><b>Battle in Progress (0 / 5)</b></color>";
            }

            if (settingsExitHallwayButton != null)
            {
                var img = settingsExitHallwayButton.GetComponent<Image>();
                if (img != null && pauseBtnRetreatNorm != null)
                {
                    img.sprite = pauseBtnRetreatNorm;
                    var ss = settingsExitHallwayButton.spriteState;
                    ss.highlightedSprite = pauseBtnRetreatHov;
                    ss.pressedSprite = pauseBtnRetreatHov;
                    ss.selectedSprite = pauseBtnRetreatHov;
                    settingsExitHallwayButton.spriteState = ss;
                }

                var txt = settingsExitHallwayButton.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) txt.gameObject.SetActive(false);

                settingsExitHallwayButton.onClick.RemoveAllListeners();
                settingsExitHallwayButton.onClick.AddListener(OnPauseExitClicked);
            }
        }

        private void EnsurePauseUIReferences()
        {
            if (settingsModal == null) return;
            var windowTr = settingsModal.transform.Find("Window");
            if (windowTr == null) return;

#if UNITY_EDITOR
            if (pauseBtnResumeNorm == null) pauseBtnResumeNorm = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Pause/Pause_Btn_Resume_Normal.png");
            if (pauseBtnResumeHov == null) pauseBtnResumeHov = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Pause/Pause_Btn_Resume_Hover.png");
            if (pauseBtnRetreatNorm == null) pauseBtnRetreatNorm = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Pause/Pause_Btn_Retreat_Normal.png");
            if (pauseBtnRetreatHov == null) pauseBtnRetreatHov = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Pause/Pause_Btn_Retreat_Hover.png");
            if (pauseConfirmFrame == null) pauseConfirmFrame = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Pause/Pause_Confirm_Frame.png");
            if (pauseBtnConfirmNorm == null) pauseBtnConfirmNorm = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Pause/Pause_Btn_Confirm_Normal.png");
            if (pauseBtnConfirmHov == null) pauseBtnConfirmHov = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Pause/Pause_Btn_Confirm_Hover.png");
            if (pauseBtnCancelNorm == null) pauseBtnCancelNorm = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Pause/Pause_Btn_Cancel_Normal.png");
            if (pauseBtnCancelHov == null) pauseBtnCancelHov = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Pause/Pause_Btn_Cancel_Hover.png");
#endif

            // Hide Title text (the scroll frame already has PAUSE / MENU carved on it)
            var titleTr = windowTr.Find("Title");
            if (titleTr != null)
            {
                titleTr.gameObject.SetActive(false);
            }

            // Find or Create StatusText
            var statusTr = windowTr.Find("StatusText");
            if (statusTr == null)
            {
                var sGo = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
                sGo.transform.SetParent(windowTr, false);
                statusTr = sGo.transform;
                var sRt = statusTr.GetComponent<RectTransform>();
                sRt.anchorMin = new Vector2(0.5f, 0.5f);
                sRt.anchorMax = new Vector2(0.5f, 0.5f);
                sRt.pivot = new Vector2(0.5f, 0.5f);
                sRt.anchoredPosition = new Vector2(0f, 85f);
                sRt.sizeDelta = new Vector2(460f, 40f);

                _pauseStatusText = sGo.GetComponent<TextMeshProUGUI>();
                _pauseStatusText.alignment = TextAlignmentOptions.Center;
                _pauseStatusText.fontSize = 15f;
                _pauseStatusText.raycastTarget = false;
            }
            else
            {
                _pauseStatusText = statusTr.GetComponent<TMP_Text>();
                if (_pauseStatusText != null)
                {
                    var sRt = statusTr.GetComponent<RectTransform>();
                    sRt.anchoredPosition = new Vector2(0f, 85f);
                    sRt.sizeDelta = new Vector2(460f, 40f);
                    _pauseStatusText.alignment = TextAlignmentOptions.Center;
                    _pauseStatusText.fontSize = 15f;
                }
            }

            // Adjust ResumeButton position
            var resumeTr = windowTr.Find("ResumeButton");
            if (resumeTr != null)
            {
                var rRt = resumeTr.GetComponent<RectTransform>();
                rRt.anchoredPosition = new Vector2(0f, 22f);
                rRt.sizeDelta = new Vector2(280f, 88f);
                var rImg = resumeTr.GetComponent<Image>();
                if (rImg != null)
                {
                    rImg.preserveAspect = true;
                    if (pauseBtnResumeNorm != null) rImg.sprite = pauseBtnResumeNorm;
                }
                var rBtn = resumeTr.GetComponent<Button>();
                if (rBtn != null && pauseBtnResumeHov != null)
                {
                    var ss = rBtn.spriteState;
                    ss.highlightedSprite = pauseBtnResumeHov;
                    ss.pressedSprite = pauseBtnResumeHov;
                    ss.selectedSprite = pauseBtnResumeHov;
                    rBtn.spriteState = ss;
                }
                var rTxt = resumeTr.Find("Text");
                if (rTxt != null) rTxt.gameObject.SetActive(false);
            }

            // Adjust ExitButton position & wire
            var exitTr = windowTr.Find("ExitButton");
            if (exitTr != null)
            {
                var eRt = exitTr.GetComponent<RectTransform>();
                eRt.anchoredPosition = new Vector2(0f, -66f);
                eRt.sizeDelta = new Vector2(280f, 88f);
                var eImg = exitTr.GetComponent<Image>();
                if (eImg != null)
                {
                    eImg.preserveAspect = true;
                    if (pauseBtnRetreatNorm != null) eImg.sprite = pauseBtnRetreatNorm;
                }

                if (settingsExitHallwayButton == null)
                    settingsExitHallwayButton = exitTr.GetComponent<Button>();

                if (settingsExitHallwayButton != null)
                {
                    var ss = settingsExitHallwayButton.spriteState;
                    ss.highlightedSprite = pauseBtnRetreatHov;
                    ss.pressedSprite = pauseBtnRetreatHov;
                    ss.selectedSprite = pauseBtnRetreatHov;
                    settingsExitHallwayButton.spriteState = ss;
                    settingsExitHallwayButton.onClick.RemoveAllListeners();
                    settingsExitHallwayButton.onClick.AddListener(OnPauseExitClicked);
                }
                var eTxt = exitTr.Find("Text");
                if (eTxt != null) eTxt.gameObject.SetActive(false);
            }

            // Find or create ConfirmPauseGroup under settingsModal directly (sibling of Window)
            var oldConfTr = windowTr.Find("ConfirmPauseGroup");
            if (oldConfTr != null)
            {
                DestroyImmediate(oldConfTr.gameObject);
            }

            var confTr = settingsModal.transform.Find("ConfirmPauseGroup");
            if (confTr == null)
            {
                var cGo = new GameObject("ConfirmPauseGroup", typeof(RectTransform), typeof(Image));
                cGo.transform.SetParent(settingsModal.transform, false);
                confTr = cGo.transform;
            }
            var cRt = confTr.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(580f, 580f);

            var cImg = confTr.GetComponent<Image>();
            if (cImg == null) cImg = confTr.gameObject.AddComponent<Image>();
            if (pauseConfirmFrame != null) cImg.sprite = pauseConfirmFrame;
            cImg.preserveAspect = true;
            cImg.color = Color.white;

            // ConfirmMessage
            var cMsgTr = confTr.Find("ConfirmMessage");
            if (cMsgTr == null)
            {
                var cMsgGo = new GameObject("ConfirmMessage", typeof(RectTransform), typeof(TextMeshProUGUI));
                cMsgGo.transform.SetParent(confTr, false);
                cMsgTr = cMsgGo.transform;
            }
            var cmRt = cMsgTr.GetComponent<RectTransform>();
            cmRt.anchorMin = new Vector2(0.5f, 0.5f);
            cmRt.anchorMax = new Vector2(0.5f, 0.5f);
            cmRt.pivot = new Vector2(0.5f, 0.5f);
            cmRt.anchoredPosition = new Vector2(0f, 32f);
            cmRt.sizeDelta = new Vector2(390f, 170f);
            _confirmMessageText = cMsgTr.GetComponent<TMP_Text>();
            if (_confirmMessageText != null)
            {
                _confirmMessageText.fontSize = 16f;
                _confirmMessageText.fontStyle = FontStyles.Bold;
                _confirmMessageText.alignment = TextAlignmentOptions.Center;
                _confirmMessageText.color = new Color(0.16f, 0.08f, 0.04f, 1f);
                _confirmMessageText.lineSpacing = 4f;
                if (string.IsNullOrEmpty(_confirmMessageText.text))
                {
                    _confirmMessageText.text = "You have not recovered any artifacts yet.\n\nRetreating will return you to the hallway without saving progress.\n\nAre you sure you want to retreat to the hallway?";
                }
            }

            // Confirm Exit Button (CONFIRM)
            var ceBtnTr = confTr.Find("ConfirmExitButton");
            if (ceBtnTr == null)
            {
                var ceBtnGo = new GameObject("ConfirmExitButton", typeof(RectTransform), typeof(Image), typeof(Button));
                ceBtnGo.transform.SetParent(confTr, false);
                ceBtnTr = ceBtnGo.transform;
            }
            var cebRt = ceBtnTr.GetComponent<RectTransform>();
            cebRt.anchorMin = new Vector2(0.5f, 0.5f);
            cebRt.anchorMax = new Vector2(0.5f, 0.5f);
            cebRt.pivot = new Vector2(0.5f, 0.5f);
            cebRt.anchoredPosition = new Vector2(0f, -87f);
            cebRt.sizeDelta = new Vector2(315f, 58f);
            var cebImg = ceBtnTr.GetComponent<Image>();
            if (cebImg == null) cebImg = ceBtnTr.gameObject.AddComponent<Image>();
            if (pauseBtnConfirmNorm != null) cebImg.sprite = pauseBtnConfirmNorm;
            cebImg.preserveAspect = true;
            cebImg.color = Color.white;
            _confirmExitButton = ceBtnTr.GetComponent<Button>();
            if (_confirmExitButton == null) _confirmExitButton = ceBtnTr.gameObject.AddComponent<Button>();
            _confirmExitButton.targetGraphic = cebImg;
            _confirmExitButton.transition = Selectable.Transition.SpriteSwap;
            if (pauseBtnConfirmHov != null)
            {
                var ss = _confirmExitButton.spriteState;
                ss.highlightedSprite = pauseBtnConfirmHov;
                ss.pressedSprite = pauseBtnConfirmHov;
                ss.selectedSprite = pauseBtnConfirmHov;
                _confirmExitButton.spriteState = ss;
            }
            _confirmExitButton.navigation = new Navigation { mode = Navigation.Mode.None };
            _confirmExitButton.onClick.RemoveAllListeners();
            _confirmExitButton.onClick.AddListener(OnReturnToHallwayClicked);
            var ceTxt = ceBtnTr.Find("Text");
            if (ceTxt != null) ceTxt.gameObject.SetActive(false);

            // Confirm Cancel Button (CANCEL)
            var ccBtnTr = confTr.Find("ConfirmCancelButton");
            if (ccBtnTr == null)
            {
                var ccBtnGo = new GameObject("ConfirmCancelButton", typeof(RectTransform), typeof(Image), typeof(Button));
                ccBtnGo.transform.SetParent(confTr, false);
                ccBtnTr = ccBtnGo.transform;
            }
            var ccbRt = ccBtnTr.GetComponent<RectTransform>();
            ccbRt.anchorMin = new Vector2(0.5f, 0.5f);
            ccbRt.anchorMax = new Vector2(0.5f, 0.5f);
            ccbRt.pivot = new Vector2(0.5f, 0.5f);
            ccbRt.anchoredPosition = new Vector2(0f, -163f);
            ccbRt.sizeDelta = new Vector2(315f, 58f);
            var ccbImg = ccBtnTr.GetComponent<Image>();
            if (ccbImg == null) ccbImg = ccBtnTr.gameObject.AddComponent<Image>();
            if (pauseBtnCancelNorm != null) ccbImg.sprite = pauseBtnCancelNorm;
            ccbImg.color = Color.white;
            ccbImg.preserveAspect = true;
            _confirmCancelButton = ccBtnTr.GetComponent<Button>();
            if (_confirmCancelButton == null) _confirmCancelButton = ccBtnTr.gameObject.AddComponent<Button>();
            _confirmCancelButton.targetGraphic = ccbImg;
            _confirmCancelButton.transition = Selectable.Transition.SpriteSwap;
            if (pauseBtnCancelHov != null)
            {
                var ccSs = _confirmCancelButton.spriteState;
                ccSs.highlightedSprite = pauseBtnCancelHov;
                ccSs.pressedSprite = pauseBtnCancelHov;
                ccSs.selectedSprite = pauseBtnCancelHov;
                _confirmCancelButton.spriteState = ccSs;
            }
            _confirmCancelButton.navigation = new Navigation { mode = Navigation.Mode.None };
            _confirmCancelButton.onClick.RemoveAllListeners();
            _confirmCancelButton.onClick.AddListener(OnCancelExitClicked);
            var ccTxt = ccBtnTr.Find("Text");
            if (ccTxt != null) ccTxt.gameObject.SetActive(false);

            _confirmPauseGroup = confTr.gameObject;
            _confirmPauseGroup.SetActive(false);
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
