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
            // Default selection
            selectedCharacter = PlayerPrefs.GetString("SelectedCharacter", "Boy");
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
            UpdateUI();
        }

        public void SelectGirl()
        {
            selectedCharacter = "Girl";
            UpdateUI();
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

            if (selectedNameText != null)
            {
                selectedNameText.text = isBoy ? "CHOSEN: BOY" : "CHOSEN: GIRL";
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
            PlayerPrefs.SetString("SelectedCharacter", selectedCharacter);
            PlayerPrefs.Save();
            Debug.Log($"Character chosen: {selectedCharacter}. Starting game...");
            SceneManager.LoadScene(nextSceneName);
        }

        public void OnBackClicked()
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
