using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace TopDownGame
{
    /// <summary>
    /// Manages room-specific behavior, entrance UI banner, and exit interaction.
    /// </summary>
    public class RoomController : MonoBehaviour
    {
        [Header("Room Identity")]
        [SerializeField] private int roomNumber = 1;
        [SerializeField] private string roomTitle = "Room 1";

        [Header("UI References")]
        [SerializeField] private TMP_Text roomBannerText;
        [SerializeField] private CanvasGroup roomBannerGroup;
        [SerializeField] private TMP_Text interactPromptText;

        [Header("Exit Configuration")]
        [SerializeField] private string lobbySceneName = "Lobby";

        public int RoomNumber => roomNumber;
        public string RoomTitle => roomTitle;

        private void Start()
        {
            if (roomBannerText != null)
            {
                roomBannerText.text = roomTitle.ToUpper();
            }

            if (roomBannerGroup != null)
            {
                StartCoroutine(BannerFadeRoutine());
            }

            // Wire DoorInteractor prompt if present
            var interactor = FindFirstObjectByType<DoorInteractor>();
            if (interactor != null && interactPromptText != null)
            {
                interactor.SetPromptLabel(interactPromptText);
            }
        }

        private IEnumerator BannerFadeRoutine()
        {
            roomBannerGroup.alpha = 1f;
            yield return new WaitForSeconds(2.0f);
            float elapsed = 0f;
            float duration = 1.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                roomBannerGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            roomBannerGroup.alpha = 0f;
        }

        public void ExitToLobby()
        {
            RoomTransitionManager.ExitToLobby();
        }

        public void SetRoomIdentity(int number, string title)
        {
            roomNumber = number;
            roomTitle = title;
            if (roomBannerText != null)
                roomBannerText.text = roomTitle.ToUpper();
        }
    }
}
