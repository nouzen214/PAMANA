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
                // Room 1 is LOCKED once the battle is completed (player cannot re-enter).
                // Room 2 is UNLOCKED only after Room 1 artifacts are brought inside Room 1 in the Real World!
                // Subsequent rooms unlock after bringing the preceding room's artifacts inside that room in the Real World.
                if (roomNum == 1)
                {
                    isLocked = GameProgression.IsRoom1BattleCompleted;
                }
                else if (roomNum == 2)
                {
                    isLocked = !GameProgression.HasReturnedRoom1ArtifactToRealWorld;
                }
                else
                {
                    isLocked = !GameProgression.HasReturnedRoomArtifactsToRealWorld(roomNum - 1);
                }
            }
            else
            {
                // In Normal World:
                // Room 1 is UNLOCKED.
                // Rooms 2 to 12 are LOCKED.
                if (roomNum == 1)
                {
                    isLocked = false;
                }
                else
                {
                    isLocked = true;
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
                    if (roomNum == 1 && GameProgression.IsRoom1BattleCompleted)
                    {
                        lockMessage = "Room 1 trial is already finished!\nYou cannot re-enter.";
                    }
                    else if (roomNum == 2 && !GameProgression.HasReturnedRoom1ArtifactToRealWorld)
                    {
                        if (GameProgression.HasRetrievedRoom1Artifact)
                        {
                            lockMessage = "Room 2 is locked!\nEnter the portal and bring the recovered artifacts inside Room 1 in the Real World first!";
                        }
                        else
                        {
                            lockMessage = "Room 2 is locked!\nConquer Room 1 trial in the Alterworld first!";
                        }
                    }
                    else
                    {
                        int prevRoom = roomNum - 1;
                        lockMessage = $"{roomDisplayName} is currently locked.\nRecover Room {prevRoom} artifacts and bring them inside Room {prevRoom} in the Real World first!";
                    }
                }
                else
                {
                    lockMessage = $"{roomDisplayName} is currently locked.\nExplore Room 1 first!";
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
