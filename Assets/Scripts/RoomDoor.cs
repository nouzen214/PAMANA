using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// Attach to door triggers in the Lobby.
    /// Inherits from DoorController so DoorInteractor detects it automatically.
    /// Pressing interact loads the target room scene and saves the door's return position.
    /// </summary>
    public class RoomDoor : DoorController
    {
        [Header("Room Target")]
        [Tooltip("The name of the room scene to load (e.g. Room_1)")]
        [SerializeField] private string targetSceneName = "Room_1";

        [Tooltip("Display name for UI prompts and banners (e.g. Room 1)")]
        [SerializeField] private string roomDisplayName = "Room 1";

        [Tooltip("Offset relative to this door where player should spawn upon returning to Lobby")]
        [SerializeField] private Vector2 returnOffset = new Vector2(0f, -1.8f);

        [Header("Lock Settings")]
        [Tooltip("If true, player cannot enter this room and will see a locked prompt.")]
        [SerializeField] private bool isLocked = false;

        public string TargetSceneName => targetSceneName;
        public string RoomDisplayName => roomDisplayName;
        public bool IsLocked
        {
            get => isLocked;
            set => isLocked = value;
        }

        private void Awake()
        {
            UpdateLockState();
        }

        private void Start()
        {
            UpdateLockState();
        }

        private void OnEnable()
        {
            GameProgression.OnRoom1ArtifactRetrieved += UpdateLockState;
            GameProgression.OnRoom1BattleCompleted += UpdateLockState;
            GameProgression.OnRoom1ArtifactReturned += UpdateLockState;
        }

        private void OnDisable()
        {
            GameProgression.OnRoom1ArtifactRetrieved -= UpdateLockState;
            GameProgression.OnRoom1BattleCompleted -= UpdateLockState;
            GameProgression.OnRoom1ArtifactReturned -= UpdateLockState;
        }

        public void UpdateLockState()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isAltScene = sceneName.StartsWith("Alt");
            int roomNum = GetRoomNumber();

            if (isAltScene)
            {
                // In Alter World:
                // Completed room battle: LOCKED (player cannot re-enter).
                // Next room: UNLOCKED only after previous room artifacts are brought inside that room in the Real World!
                if (GameProgression.IsRoomBattleCompleted(roomNum))
                {
                    isLocked = true;
                }
                else if (roomNum == 1)
                {
                    isLocked = false;
                }
                else
                {
                    isLocked = !GameProgression.HasReturnedRoomArtifactsToRealWorld(roomNum - 1);
                }
            }
            else
            {
                // In Normal World:
                // Room 1 is UNLOCKED from the start.
                // Rooms 2..12 unlock when the player has retrieved their artifacts from the Alterworld or already returned them.
                if (roomNum == 1)
                {
                    isLocked = false;
                }
                else
                {
                    isLocked = !GameProgression.HasRetrievedRoomArtifact(roomNum) && !GameProgression.HasReturnedRoomArtifactsToRealWorld(roomNum);
                }
            }
        }

        public int GetRoomNumber()
        {
            var match = System.Text.RegularExpressions.Regex.Match(roomDisplayName, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int parsed))
            {
                return parsed;
            }
            return 1;
        }

        public override void Interact()
        {
            UpdateLockState();

            if (isLocked)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                bool isAltScene = sceneName.StartsWith("Alt");
                int roomNum = GetRoomNumber();

                string lockMessage;
                if (isAltScene)
                {
                    if (GameProgression.IsRoomBattleCompleted(roomNum))
                    {
                        lockMessage = $"{roomDisplayName} trial is already finished!\nYou cannot re-enter.";
                    }
                    else
                    {
                        int prevRoom = roomNum - 1;
                        if (GameProgression.HasRetrievedRoomArtifact(prevRoom))
                        {
                            lockMessage = $"{roomDisplayName} is locked!\nEnter the portal and bring the recovered artifacts inside Room {prevRoom} in the Real World first!";
                        }
                        else
                        {
                            lockMessage = $"{roomDisplayName} is locked!\nConquer Room {prevRoom} trial in the Alterworld first!";
                        }
                    }
                }
                else
                {
                    lockMessage = $"{roomDisplayName} is currently locked.\nConquer Room {roomNum} trial in the Alterworld to recover its artifacts first!";
                }

                NotificationBannerUI.Show(
                    "DOOR LOCKED",
                    lockMessage,
                    2.8f,
                    new Color(0.95f, 0.35f, 0.35f, 1f),
                    "!"
                );
                return;
            }

            EnterRoom();
        }

        public override string GetPromptText(string openText, string closeText)
        {
            UpdateLockState();

            if (isLocked)
            {
                return $"[Locked] {roomDisplayName}";
            }

            return $"[E] Enter {roomDisplayName}";
        }

        public void EnterRoom()
        {
            Vector2 spawnPos = (Vector2)transform.position + returnOffset;
            RoomTransitionManager.CurrentRoomDisplayName = roomDisplayName;
            int roomNum = GetRoomNumber();
            RoomTransitionManager.CurrentRoomNumber = roomNum;
            RoomTransitionManager.EnterRoom(targetSceneName, gameObject.name, spawnPos);
        }

        public void SetConfig(string sceneName, string displayName, Vector2 offset, bool locked = false)
        {
            targetSceneName = sceneName;
            roomDisplayName = displayName;
            returnOffset = offset;
            isLocked = locked;
        }
    }
}
