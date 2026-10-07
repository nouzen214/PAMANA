using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TopDownGame
{
    public class InGameMenuController : MonoBehaviour
    {
        [Header("Scene Config")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;

        [Header("Audio (Optional)")]
        [SerializeField] private Slider volumeSlider;

        private void Start()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }
        }

        public void OnSettingsClicked()
        {
            if (settingsPanel != null)
            {
                bool willOpen = !settingsPanel.activeSelf;
                settingsPanel.SetActive(willOpen);
                Time.timeScale = willOpen ? 0f : 1f;
            }
            else
            {
                // Fallback if no modal is hooked up: go directly to main menu
                GoToMainMenu();
            }
        }

        public void OnCloseSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
            Time.timeScale = 1f;
        }

        public void OnBagClicked()
        {
            Debug.Log("[InGameMenuController] Bag / Inventory button clicked!");
        }

        public void OnBookClicked()
        {
            Debug.Log("[InGameMenuController] Book / Journal button clicked!");
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void RestartCurrentScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}

