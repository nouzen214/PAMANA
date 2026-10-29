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
        public int RoomNumber => roomNumber;
        public string RoomTitle => roomTitle;

        private void Start()
        {
            if (SceneManager.GetActiveScene().name.StartsWith("BattleScene") && !string.IsNullOrEmpty(RoomTransitionManager.CurrentRoomDisplayName))
            {
                roomTitle = RoomTransitionManager.CurrentRoomDisplayName;
                roomNumber = RoomTransitionManager.CurrentRoomNumber;
            }

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

            CheckArtifactReturn(SceneManager.GetActiveScene().name);
        }

        private void CheckArtifactReturn(string sceneName)
        {
            int currentRoom = roomNumber;
            if (currentRoom <= 0 && sceneName.StartsWith("Room_"))
            {
                int.TryParse(sceneName.Substring(5), out currentRoom);
            }

            // Real World Rooms
            if (!sceneName.StartsWith("Alt") && !sceneName.StartsWith("BattleScene") && currentRoom > 0)
            {
                // Update visuals on every pedestal according to individual restoration state
                UpdateRoomPedestalVisuals(currentRoom);

                if (GameProgression.HasReturnedRoomArtifactsToRealWorld(currentRoom))
                {
                    // Already fully restored
                }
                else if (GameProgression.HasRetrievedRoomArtifact(currentRoom))
                {
                    int retrieved = GameProgression.GetRoomRetrievedCount(currentRoom);
                    int restored = GameProgression.GetPedestalsRestoredCount(currentRoom);

                    NotificationBannerUI.Show(
                        $"ROOM {currentRoom}",
                        $"You have recovered artifacts in your bag!\nWalk up to each pedestal and interact to place them ({restored}/{retrieved} placed).",
                        5.5f,
                        new Color(0.2f, 0.92f, 0.5f, 1f),
                        "★"
                    );
                }
                else if (!GameProgression.AreAllPlaquesRead(currentRoom))
                {
                    int readCount = GameProgression.GetPlaquesReadCount(currentRoom);
                    NotificationBannerUI.Show(
                        $"ROOM {currentRoom}",
                        $"Read all 5 artifact plaques in this room to unlock the Alterworld trial! ({readCount}/5 studied)",
                        4.5f,
                        new Color(1f, 0.88f, 0.4f, 1f),
                        "i"
                    );
                }
            }
        }

        public void UpdateRoomPedestalVisuals(int roomNum)
        {
            var tables = FindObjectsByType<ArtifactInteractable>(FindObjectsSortMode.None);
            foreach (var table in tables)
            {
                if (table != null)
                {
                    bool isRestored = GameProgression.IsPedestalRestored(roomNum, table.ArtifactName);
                    table.SetRestoredVisual(isRestored);
                }
            }
        }

        private void RestoreTableArtifactVisuals()
        {
            UpdateRoomPedestalVisuals(roomNumber);
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
