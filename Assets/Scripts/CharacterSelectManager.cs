using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    public class CharacterSelectManager : MonoBehaviour
    {
        [Header("Character Selection")]
        [SerializeField] private string selectedCharacter = "Boy"; // "Boy" or "Girl"
        [SerializeField] private string nextSceneName = "Lobby";
        [SerializeField] private string menuSceneName = "MainMenu";

        [Header("UI Cards")]
        [SerializeField] private Image boyCardBackground;
        [SerializeField] private Image girlCardBackground;
        [SerializeField] private Image boyPreviewImage;
        [SerializeField] private Image girlPreviewImage;
        [SerializeField] private TextMeshProUGUI selectedNameText;
        [SerializeField] private TextMeshProUGUI characterDescriptionText;

        [Header("Character Name Input")]
        [SerializeField] private TMP_InputField nameInputField;

        [Header("Colors")]
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color unselectedColor = new Color(0.55f, 0.55f, 0.58f, 0.88f);

        [Header("Animation Preview")]
        [SerializeField] private Sprite[] boyWalkFrames;
        [SerializeField] private Sprite[] girlWalkFrames;
        [SerializeField] private float previewFrameRate = 6f;

        private float _animTimer;
        private int _frameIdx;

        private void Start()
        {
            // If character was already selected, do not make the user pick again
            if (PlayerPrefs.GetInt("HasSelectedCharacter", 0) == 1)
            {
                string chosen = PlayerPrefs.GetString("SelectedCharacter", selectedCharacter);
                Debug.Log($"[CharacterSelectManager] Character already selected ({chosen}). Proceeding to game...");
                SceneManager.LoadScene(nextSceneName);
                return;
            }

            // Default selection for first-time player
            selectedCharacter = PlayerPrefs.GetString("SelectedCharacter", "Boy");

            string defaultName = selectedCharacter == "Girl" ? "Darla" : "Darrel";
            string savedName = PlayerPrefs.GetString("CustomCharacterName", "");

            if (string.IsNullOrEmpty(savedName) && FirebaseAuthManager.Instance != null &&
                FirebaseAuthManager.Instance.IsGoogleUser && !string.IsNullOrEmpty(FirebaseAuthManager.Instance.Username))
            {
                savedName = FirebaseAuthManager.Instance.Username;
            }

            if (nameInputField != null)
            {
                nameInputField.characterLimit = 16;
                nameInputField.text = !string.IsNullOrEmpty(savedName) ? savedName : defaultName;
                nameInputField.onValueChanged.RemoveAllListeners();
                nameInputField.onValueChanged.AddListener(OnNameInputChanged);
            }

            UpdateUI();
        }

        private void OnNameInputChanged(string val)
        {
            UpdateUI();
        }

        private void Update()
        {
            // Animated preview for both characters with natural 4-step cycle
            _animTimer += Time.deltaTime;
            if (_animTimer >= 1f / previewFrameRate)
            {
                _animTimer = 0f;
                _frameIdx++;

                int[] stepSeq = { 1, 0, 2, 0 };

                if (boyPreviewImage != null && boyWalkFrames != null && boyWalkFrames.Length > 0)
                {
                    int index = boyWalkFrames.Length >= 3 ? stepSeq[_frameIdx % stepSeq.Length] : (_frameIdx % boyWalkFrames.Length);
                    if (index < boyWalkFrames.Length && boyWalkFrames[index] != null)
                    {
                        boyPreviewImage.sprite = boyWalkFrames[index];
                    }
                }

                if (girlPreviewImage != null && girlWalkFrames != null && girlWalkFrames.Length > 0)
                {
                    int index = girlWalkFrames.Length >= 3 ? stepSeq[_frameIdx % stepSeq.Length] : (_frameIdx % girlWalkFrames.Length);
                    if (index < girlWalkFrames.Length && girlWalkFrames[index] != null)
                    {
                        girlPreviewImage.sprite = girlWalkFrames[index];
                    }
                }
            }
        }

        public void SelectBoy()
        {
            selectedCharacter = "Boy";
            UpdateNameForCharacter("Darrel", "Darla");
            UpdateUI();
        }

        public void SelectGirl()
        {
            selectedCharacter = "Girl";
            UpdateNameForCharacter("Darla", "Darrel");
            UpdateUI();
        }

        private void UpdateNameForCharacter(string newDefault, string oldDefault)
        {
            if (nameInputField != null)
            {
                string currentText = nameInputField.text.Trim();
                if (string.IsNullOrEmpty(currentText) || string.Equals(currentText, oldDefault, System.StringComparison.OrdinalIgnoreCase))
                {
                    nameInputField.text = newDefault;
                }
            }
        }

        private void UpdateUI()
        {
            bool isBoy = selectedCharacter == "Boy";

            if (boyCardBackground != null)
            {
                boyCardBackground.color = isBoy ? selectedColor : unselectedColor;
                boyCardBackground.transform.localScale = isBoy ? Vector3.one * 1.05f : Vector3.one;

                var boyName = boyCardBackground.GetComponentInChildren<TextMeshProUGUI>();
                if (boyName != null) boyName.color = isBoy ? new Color(1f, 0.92f, 0.6f, 1f) : new Color(0.75f, 0.72f, 0.7f, 0.8f);
            }

            if (girlCardBackground != null)
            {
                girlCardBackground.color = !isBoy ? selectedColor : unselectedColor;
                girlCardBackground.transform.localScale = !isBoy ? Vector3.one * 1.05f : Vector3.one;

                var girlName = girlCardBackground.GetComponentInChildren<TextMeshProUGUI>();
                if (girlName != null) girlName.color = !isBoy ? new Color(1f, 0.92f, 0.6f, 1f) : new Color(0.75f, 0.72f, 0.7f, 0.8f);
            }

            if (boyPreviewImage != null)
            {
                boyPreviewImage.color = isBoy ? Color.white : new Color(0.72f, 0.72f, 0.75f, 0.88f);
            }

            if (girlPreviewImage != null)
            {
                girlPreviewImage.color = !isBoy ? Color.white : new Color(0.72f, 0.72f, 0.75f, 0.88f);
            }

            string chosenName = nameInputField != null ? nameInputField.text.Trim() : "";
            if (string.IsNullOrEmpty(chosenName))
            {
                chosenName = isBoy ? "Darrel" : "Darla";
            }

            if (selectedNameText != null && selectedNameText.gameObject.activeSelf)
            {
                selectedNameText.gameObject.SetActive(false);
            }

            if (characterDescriptionText != null)
            {
                characterDescriptionText.text = isBoy
                    ? "Curious student explorer uncovering ancient Philippine heritage."
                    : "Bright student explorer ready to solve museum mysteries.";
            }
        }

        public void OnConfirmAndPlay()
        {
            string fallback = selectedCharacter == "Girl" ? "Darla" : "Darrel";
            string rawInput = nameInputField != null ? nameInputField.text : "";
            string chosenName = SanitizeAndValidateName(rawInput, fallback);

            PlayerPrefs.SetString("SelectedCharacter", selectedCharacter);
            PlayerPrefs.SetString("CustomCharacterName", chosenName);
            PlayerPrefs.SetString("PlayerUsername", chosenName);
            PlayerPrefs.SetInt("HasSelectedCharacter", 1);
            PlayerPrefs.Save();
            GameProgression.ResetProgression();
            PlayerInventory.Clear();
            Debug.Log($"Character chosen: {selectedCharacter}, Name: '{chosenName}'. Starting game...");

            // Persist to Firebase so returning logins remember the choice
            if (FirebaseDatabaseManager.Instance != null)
            {
                var data = FirebaseDatabaseManager.Instance.CurrentPlayerData ?? new PlayerData();
                data.selectedCharacter = selectedCharacter;
                data.characterName = chosenName;
                data.username = chosenName;
                data.hasSelectedCharacter = true;
                data.currentScene = nextSceneName;
                FirebaseDatabaseManager.Instance.SavePlayerData(data, null);
            }

            SceneManager.LoadScene(nextSceneName);
        }

        public static string SanitizeAndValidateName(string input, string defaultFallback)
        {
            if (string.IsNullOrWhiteSpace(input)) return defaultFallback;

            string trimmed = input.Trim();

            // 1. Length constraint: 1 to 15 characters
            if (trimmed.Length > 15)
            {
                trimmed = trimmed.Substring(0, 15).Trim();
            }

            // 2. Character whitelist: letters, digits, spaces, hyphens, and underscores only
            var sb = new System.Text.StringBuilder();
            foreach (char c in trimmed)
            {
                if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_')
                {
                    sb.Append(c);
                }
            }

            string sanitized = sb.ToString().Trim();
            return string.IsNullOrEmpty(sanitized) ? defaultFallback : sanitized;
        }

        public void OnBackClicked()
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
