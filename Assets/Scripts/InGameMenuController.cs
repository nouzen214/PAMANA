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
        [SerializeField] private BagModalUI bagModal;

        [Header("Audio (Optional)")]
        [SerializeField] private Slider volumeSlider;

        private void Start()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (bagModal == null)
            {
                bagModal = FindFirstObjectByType<BagModalUI>(FindObjectsInactive.Include);
            }

            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }
        }

        public void OnSettingsClicked()
        {
            if (bagModal != null && bagModal.IsOpen)
            {
                bagModal.CloseBag();
            }

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
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
                Time.timeScale = 1f;
            }

            if (bagModal == null)
            {
                bagModal = FindFirstObjectByType<BagModalUI>(FindObjectsInactive.Include);
            }

            if (bagModal == null)
            {
                bagModal = BagModalUI.CreateBagModal(transform);
            }

            if (bagModal != null)
            {
                if (bagModal.IsOpen)
                {
                    bagModal.CloseBag();
                }
                else
                {
                    bagModal.OpenBag();
                }
            }
        }

        public void OnBookClicked()
        {
            if (bagModal != null && bagModal.IsOpen)
            {
                bagModal.CloseBag();
            }

            TutorialModalUI.Show();
        }

        public void OnTutorialClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
            if (bagModal != null && bagModal.IsOpen)
            {
                bagModal.CloseBag();
            }
            TutorialModalUI.Show();
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
