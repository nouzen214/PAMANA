using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// Attached to the Book Pedestal in the Main Lobby.
    /// Inherits from DoorController so Player's DoorInteractor detects it.
    /// Interacting with it unlocks the Field Guide (Book) and opens the guide modal.
    /// </summary>
    public class BookPedestal : DoorController
    {
        [Header("Book Visual")]
        [SerializeField] private SpriteRenderer bookSpriteRenderer;
        [SerializeField] private Sprite bookSprite;

        [Header("Prompt Settings")]
        [SerializeField] private string lockedPrompt = "[E] Inspect Field Guide";
        [SerializeField] private string unlockedPrompt = "[E] Read Field Guide";

        private void Start()
        {
            UpdateVisuals();
        }

        private void OnEnable()
        {
            GameProgression.OnBookUnlocked += UpdateVisuals;
        }

        private void OnDisable()
        {
            GameProgression.OnBookUnlocked -= UpdateVisuals;
        }

        public void UpdateVisuals()
        {
            if (bookSpriteRenderer != null && bookSprite != null)
            {
                bookSpriteRenderer.sprite = bookSprite;
                bookSpriteRenderer.color = Color.white;

                var mySr = GetComponent<SpriteRenderer>();
                if (mySr != null)
                {
                    bookSpriteRenderer.sortingLayerID = mySr.sortingLayerID;
                    bookSpriteRenderer.sortingOrder = mySr.sortingOrder + 2;
                }
            }
        }

        private void LateUpdate()
        {
            if (bookSpriteRenderer != null)
            {
                var mySr = GetComponent<SpriteRenderer>();
                if (mySr != null)
                {
                    bookSpriteRenderer.sortingLayerID = mySr.sortingLayerID;
                    bookSpriteRenderer.sortingOrder = mySr.sortingOrder + 2;
                }
            }
        }

        public override void Interact()
        {
            if (!GameProgression.IsBookUnlocked)
            {
                GameProgression.UnlockBook();

                NotificationBannerUI.Show(
                    "FIELD GUIDE UNLOCKED!",
                    "You unlocked the Museum Field Guide! Access it anytime via the book icon in your HUD.",
                    5.5f,
                    new Color(0.18f, 0.40f, 0.16f, 1f),
                    "★"
                );
            }

            BookModalUI.Show();
        }

        public override string GetPromptText(string openText, string closeText)
        {
            return GameProgression.IsBookUnlocked ? unlockedPrompt : lockedPrompt;
        }

        public void Configure(SpriteRenderer bookRenderer, Sprite bookSpr)
        {
            bookSpriteRenderer = bookRenderer;
            bookSprite = bookSpr;
            UpdateVisuals();
        }
    }
}

