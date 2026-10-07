using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TopDownGame
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Management")]
        [SerializeField] private string gameSceneName = "CharacterSelect";

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;

        [Header("Audio Settings (Optional)")]
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

        public void OnStartGameClicked()
        {
            Debug.Log("Starting Game...");
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }

        public void OnSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void OnCloseSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }

        public void OnExitClicked()
        {
            Debug.Log("Exit Game clicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            // WebGL cannot Application.Quit(), we can redirect or show a thank you message
            Application.ExternalEval("window.location.reload();");
#else
            Application.Quit();
#endif
        }
    }
}
