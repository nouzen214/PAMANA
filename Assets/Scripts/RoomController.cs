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
                if (GameProgression.HasRetrievedRoomArtifact(currentRoom) && !GameProgression.HasReturnedRoomArtifactsToRealWorld(currentRoom))
                {
                    bool justReturned = GameProgression.ReturnRoomArtifactsToRealWorld(currentRoom);
                    if (justReturned)
                    {
                        NotificationBannerUI.Show(
                            $"ARTIFACTS RETURNED TO ROOM {currentRoom}!",
                            $"You brought the artifacts back inside Room {currentRoom}! Alterworld Room {currentRoom + 1} is now UNLOCKED!",
                            6.0f,
                            new Color(0.2f, 0.95f, 0.4f, 1f),
                            "★"
                        );
                    }
                }

                if (GameProgression.HasReturnedRoomArtifactsToRealWorld(currentRoom) || GameProgression.HasRetrievedRoomArtifact(currentRoom))
                {
                    RestoreTableArtifactVisuals();
                }
            }
        }

        private void RestoreTableArtifactVisuals()
        {
            var roomArtifacts = GameObject.Find("RoomArtifacts");
            if (roomArtifacts != null)
            {
                var renderers = roomArtifacts.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var sr in renderers)
                {
                    if (sr != null)
                    {
                        sr.color = Color.white; // Restored from silhouette!
                    }
                }
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
