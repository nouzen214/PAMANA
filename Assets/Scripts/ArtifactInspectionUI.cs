using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TopDownGame
{
    /// <summary>
    /// UI Controller for the Artifact Inspection modal with dialogue box,
    /// dynamic character avatar, and choice prompts ("inspect artifact" / "inspect plaque").
    /// </summary>
    public class ArtifactInspectionUI : MonoBehaviour
    {
        public static ArtifactInspectionUI Instance { get; private set; }

        [Header("Root")]
        [SerializeField] private GameObject rootPanel;

        [Header("Character Info & Avatar")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private Sprite boyAvatarSprite;
        [SerializeField] private Sprite girlAvatarSprite;
        [SerializeField] private TMP_Text characterNameText;
        [SerializeField] private TMP_Text dialogueText;

        [Header("Choice Container & Buttons")]
        [SerializeField] private GameObject choiceContainer;
        [SerializeField] private Button inspectArtifactButton;
        [SerializeField] private Button inspectPlaqueButton;

        [Header("Plaque Popup")]
        [SerializeField] private GameObject plaquePopup;
        [SerializeField] private TMP_Text plaqueTitleText;
        [SerializeField] private TMP_Text plaqueBodyText;
        [SerializeField] private Button closePlaqueButton;

        [Header("Artifact Detail Popup")]
        [SerializeField] private GameObject artifactPopup;
        [SerializeField] private Image artifactGlowImage;
        [SerializeField] private Image artifactDetailImage;
        [SerializeField] private TMP_Text artifactDetailTitleText;
        [SerializeField] private TMP_Text artifactDetailBodyText;
        [SerializeField] private Button closeArtifactDetailButton;

        [Header("Close All / Background Tap")]
        [SerializeField] private Button backgroundTapButton;

        private ArtifactInteractable _currentArtifact;
        private PlayerMovement _playerMovement;
        private bool _isOpen = false;
        private readonly System.Collections.Generic.List<GameObject> _hiddenHudObjects = new System.Collections.Generic.List<GameObject>();
        private SpriteRenderer _hiddenPlayerSr;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (inspectArtifactButton != null)
                inspectArtifactButton.onClick.AddListener(OnInspectArtifactClicked);

            if (inspectPlaqueButton != null)
                inspectPlaqueButton.onClick.AddListener(OnInspectPlaqueClicked);

            if (closePlaqueButton != null)
                closePlaqueButton.onClick.AddListener(Close);

            if (closeArtifactDetailButton != null)
                closeArtifactDetailButton.onClick.AddListener(Close);

            if (backgroundTapButton != null)
                backgroundTapButton.onClick.AddListener(Close);

            if (rootPanel != null)
                rootPanel.SetActive(false);

            if (_playerMovement == null)
                _playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (_playerMovement != null)
                _playerMovement.enabled = true;
        }

        private void Update()
        {
            if (!_isOpen) return;

            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb.escapeKey.wasPressedThisFrame)
                {
                    Close();
                    return;
                }

                if (choiceContainer != null && choiceContainer.activeSelf)
                {
                    if (kb.digit1Key.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
                    {
                        OnInspectArtifactClicked();
                    }
                    else if (kb.digit2Key.wasPressedThisFrame || kb.pKey.wasPressedThisFrame)
                    {
                        OnInspectPlaqueClicked();
                    }
                }
                else if ((plaquePopup != null && plaquePopup.activeSelf) || (artifactPopup != null && artifactPopup.activeSelf))
                {
                    if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
                    {
                        Close();
                    }
                }
            }
        }

        public void Open(ArtifactInteractable artifact)
        {
            if (Instance == null) Instance = this;
            _currentArtifact = artifact;
            _isOpen = true;

            // Pause player movement
            if (_playerMovement == null)
                _playerMovement = FindFirstObjectByType<PlayerMovement>();

            if (_playerMovement != null)
            {
                _playerMovement.enabled = false;
                var rb = _playerMovement.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;

                _hiddenPlayerSr = _playerMovement.GetComponentInChildren<SpriteRenderer>();
                if (_hiddenPlayerSr != null && _hiddenPlayerSr.enabled)
                {
                    _hiddenPlayerSr.enabled = false;
                }
                else
                {
                    _hiddenPlayerSr = null;
                }
            }

            // Hide other gameplay HUD elements (Joystick, Hearts, Room Banner, Home Button)
            _hiddenHudObjects.Clear();
            var canvasTr = rootPanel != null ? rootPanel.transform.parent : transform;
            if (canvasTr != null)
            {
                foreach (Transform child in canvasTr)
                {
                    if (child.gameObject != rootPanel && child.gameObject.activeSelf)
                    {
                        _hiddenHudObjects.Add(child.gameObject);
                        child.gameObject.SetActive(false);
                    }
                }
            }

            // Update Avatar based on character selection
            bool isGirl = false;
            var anim = FindFirstObjectByType<CharacterAnimator2D>();
            if (anim != null)
            {
                isGirl = anim.IsGirlActive;
            }
            else
            {
                isGirl = string.Equals(PlayerPrefs.GetString("SelectedCharacter", "Boy"), "Girl", System.StringComparison.OrdinalIgnoreCase);
            }

            if (avatarImage != null)
            {
                avatarImage.sprite = isGirl ? girlAvatarSprite : boyAvatarSprite;
            }

            if (characterNameText != null)
            {
                characterNameText.text = isGirl ? "Darla" : "Darrel";
                characterNameText.color = new Color(1f, 0.95f, 0.85f, 1f);
            }

            if (dialogueText != null)
            {
                dialogueText.text = artifact != null ? artifact.InitialDialogue : "what should i do";
                dialogueText.color = new Color(0.18f, 0.18f, 0.18f, 1f);
            }

            // Show choices, hide popups
            if (choiceContainer != null) choiceContainer.SetActive(true);
            if (plaquePopup != null) plaquePopup.SetActive(false);
            if (artifactPopup != null) artifactPopup.SetActive(false);

            if (rootPanel != null)
                rootPanel.SetActive(true);
        }

        public void OnInspectPlaqueClicked()
        {
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (artifactPopup != null) artifactPopup.SetActive(false);

            if (plaquePopup != null)
            {
                plaquePopup.SetActive(true);
                if (plaqueTitleText != null && _currentArtifact != null)
                    plaqueTitleText.text = _currentArtifact.PlaqueTitle;
                if (plaqueBodyText != null && _currentArtifact != null)
                    plaqueBodyText.text = _currentArtifact.PlaqueDescription;
            }

            if (dialogueText != null && _currentArtifact != null)
            {
                dialogueText.text = _currentArtifact.PlaqueDialogue;
            }
        }

        public void OnInspectArtifactClicked()
        {
            if (choiceContainer != null) choiceContainer.SetActive(false);
            if (plaquePopup != null) plaquePopup.SetActive(false);

            if (artifactPopup != null)
            {
                artifactPopup.SetActive(true);
                if (artifactGlowImage != null)
                {
                    artifactGlowImage.gameObject.SetActive(true);
                    artifactGlowImage.color = Color.white;
                }
                if (artifactDetailImage != null && _currentArtifact != null)
                {
                    artifactDetailImage.sprite = _currentArtifact.ArtifactSprite;
                    artifactDetailImage.color = Color.black; // Silhouette effect per reference image
                }
                if (artifactDetailTitleText != null)
                    artifactDetailTitleText.gameObject.SetActive(false);
                if (artifactDetailBodyText != null)
                    artifactDetailBodyText.gameObject.SetActive(false);
            }

            if (dialogueText != null)
            {
                string diag = (_currentArtifact != null && !string.IsNullOrEmpty(_currentArtifact.ArtifactDialogue) && _currentArtifact.ArtifactDialogue != "what could this artifact be")
                    ? _currentArtifact.ArtifactDialogue
                    : "i wonder what this artifact looks like";
                dialogueText.text = diag;
            }
        }

        public void Close()
        {
            Debug.LogWarning("[ArtifactInspectionUI] Close called! StackTrace:\n" + System.Environment.StackTrace);
            _isOpen = false;
            if (rootPanel != null)
                rootPanel.SetActive(false);

            if (plaquePopup != null) plaquePopup.SetActive(false);
            if (artifactPopup != null) artifactPopup.SetActive(false);
            if (choiceContainer != null) choiceContainer.SetActive(false);

            // Restore hidden gameplay HUD elements
            foreach (var hud in _hiddenHudObjects)
            {
                if (hud != null) hud.SetActive(true);
            }
            _hiddenHudObjects.Clear();

            // Restore player sprite
            if (_hiddenPlayerSr != null)
            {
                _hiddenPlayerSr.enabled = true;
                _hiddenPlayerSr = null;
            }

            // Re-enable player movement
            if (_playerMovement != null)
                _playerMovement.enabled = true;

            _currentArtifact = null;
        }
    }
}
