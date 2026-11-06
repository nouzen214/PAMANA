using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    public class InGameMenuController : MonoBehaviour
    {
        [Header("Scene Config")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private BagModalUI bagModal;

        [Header("Audio Settings Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;

        [Header("Google Connect")]
        [SerializeField] private Button connectGoogleButton;
        [SerializeField] private TextMeshProUGUI connectGoogleText;

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

            InitializeSliders();
        }

        private void InitializeSliders()
        {
            AudioSettingsManager.Load();

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = AudioSettingsManager.MasterVolume;
                masterVolumeSlider.onValueChanged.RemoveAllListeners();
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioSettingsManager.SFXVolume;
                sfxVolumeSlider.onValueChanged.RemoveAllListeners();
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = AudioSettingsManager.BGMVolume;
                bgmVolumeSlider.onValueChanged.RemoveAllListeners();
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
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
                if (willOpen)
                {
                    InitializeSliders();
                    UpdateGoogleConnectUI();
                }
                settingsPanel.SetActive(willOpen);
                Time.timeScale = willOpen ? 0f : 1f;
            }
            else
            {
                GoToMainMenu();
            }
        }

        public void OnConnectToGoogleClicked()
        {
            if (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsGoogleUser)
            {
                // Account is currently connected to Google -> UNBIND
                string prevUser = FirebaseAuthManager.Instance.Username;

                // Sync and save progress to the current Google account before unbinding
                if (FirebaseDatabaseManager.Instance != null)
                {
                    FirebaseDatabaseManager.Instance.SyncFromGameAndSave(null);
                }

                FirebaseAuthManager.Instance.UnbindGoogle();
                UpdateGoogleConnectUI();
                NotificationBannerUI.Show("Google Unbound", $"Disconnected from {prevUser}. You can now connect a different Google account or play as guest.");
                return;
            }

            // Not connected to Google (Playing as Guest) -> BIND TO GOOGLE
            if (connectGoogleText != null)
            {
                connectGoogleText.text = "CONNECTING...";
            }

            FirebaseAuthManager.Instance.GoogleSignIn((success, msg) =>
            {
                if (success)
                {
                    // Successfully authenticated with Google!
                    // Bind the guest's active game progress directly to this Google account in Firebase!
                    if (FirebaseDatabaseManager.Instance != null)
                    {
                        FirebaseDatabaseManager.Instance.SyncFromGameAndSave((saveSuccess, saveMsg) =>
                        {
                            UpdateGoogleConnectUI();
                            NotificationBannerUI.Show("Account Bound!", $"Connected as {FirebaseAuthManager.Instance.Username}! Progress saved to cloud.");
                        });
                    }
                    else
                    {
                        UpdateGoogleConnectUI();
                        NotificationBannerUI.Show("Account Bound!", $"Connected as {FirebaseAuthManager.Instance.Username}!");
                    }
                }
                else
                {
                    UpdateGoogleConnectUI();
                    NotificationBannerUI.Show("Google Sign-In", msg);
                }
            });
        }

        public void UpdateGoogleConnectUI()
        {
            if (connectGoogleText != null)
            {
                if (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsGoogleUser)
                {
                    connectGoogleText.text = $"UNBIND: {FirebaseAuthManager.Instance.Username.ToUpper()}";
                }
                else
                {
                    connectGoogleText.text = "CONNECT TO GOOGLE";
                }
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

        public void OnSaveSettingsClicked()
        {
            AudioSettingsManager.Save();
            OnCloseSettingsClicked();
        }

        public void OnDefaultSettingsClicked()
        {
            AudioSettingsManager.ResetDefaults();
            if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
            if (bgmVolumeSlider != null) bgmVolumeSlider.value = 1f;
        }

        public void OnMasterVolumeChanged(float volume)
        {
            AudioSettingsManager.SetMaster(volume);
        }

        public void OnSFXVolumeChanged(float volume)
        {
            AudioSettingsManager.SetSFX(volume);
        }

        public void OnBGMVolumeChanged(float volume)
        {
            AudioSettingsManager.SetBGM(volume);
        }

        public void SetVolume(float volume)
        {
            OnMasterVolumeChanged(volume);
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
