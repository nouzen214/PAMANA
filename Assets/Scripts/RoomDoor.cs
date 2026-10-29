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
            GameProgression.OnPlaqueRead += HandlePlaqueRead;
            GameProgression.OnAllPlaquesReadInRoom += HandleAllPlaquesRead;
            GameProgression.OnRoomArtifactsReturned += HandleRoomArtifactsReturned;
            GameProgression.OnPortalUnlocked += UpdateLockState;
        }

        private void OnDisable()
        {
            GameProgression.OnRoom1ArtifactRetrieved -= UpdateLockState;
            GameProgression.OnRoom1BattleCompleted -= UpdateLockState;
            GameProgression.OnRoom1ArtifactReturned -= UpdateLockState;
            GameProgression.OnPlaqueRead -= HandlePlaqueRead;
            GameProgression.OnAllPlaquesReadInRoom -= HandleAllPlaquesRead;
            GameProgression.OnRoomArtifactsReturned -= HandleRoomArtifactsReturned;
            GameProgression.OnPortalUnlocked -= UpdateLockState;
        }

        private void HandlePlaqueRead(int roomNum, int count) => UpdateLockState();
        private void HandleAllPlaquesRead(int roomNum) => UpdateLockState();
        private void HandleRoomArtifactsReturned(int roomNum) => UpdateLockState();

        public void UpdateLockState()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isAltScene = sceneName.StartsWith("Alt");
            int roomNum = GetRoomNumber();

            if (isAltScene)
            {
                // In Alter World:
                // Completed room battle: LOCKED (player cannot re-enter).
                if (GameProgression.IsRoomBattleCompleted(roomNum))
                {
                    isLocked = true;
                }
                else if (roomNum == 1)
                {
                    // Room 1 Alterworld battle unlocks ONLY when all 5 plaques in Room 1 are read!
                    isLocked = !GameProgression.AreAllPlaquesRead(1);
                }
                else
                {
                    // Next room: UNLOCKED only after:
                    // 1) previous room artifacts are restored in the Real World!
                    // 2) AND all 5 plaques in this room have been read in the Real World!
                    bool prevReturned = GameProgression.HasReturnedRoomArtifactsToRealWorld(roomNum - 1);
                    bool plaquesRead = GameProgression.AreAllPlaquesRead(roomNum);
                    isLocked = (!prevReturned || !plaquesRead);
                }
            }
            else
            {
                // In Normal World:
                // Room 1 is UNLOCKED from the start.
                // Room N (N >= 2) unlocks when the previous room (N - 1) artifacts have been restored!
                if (roomNum == 1)
                {
                    isLocked = false;
                }
                else
                {
                    isLocked = !GameProgression.HasReturnedRoomArtifactsToRealWorld(roomNum - 1);
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
                    else if (roomNum == 1)
                    {
                        int readCount = GameProgression.GetPlaquesReadCount(1);
                        lockMessage = $"Alterworld Battle 1 is locked!\nVisit Real World Room 1 and read all 5 plaques first! ({readCount}/5 studied)";
                    }
                    else
                    {
                        int prevRoom = roomNum - 1;
                        if (!GameProgression.HasReturnedRoomArtifactsToRealWorld(prevRoom))
                        {
                            if (GameProgression.HasRetrievedRoomArtifact(prevRoom))
                            {
                                lockMessage = $"{roomDisplayName} trial is locked!\nReturn the recovered artifacts inside Room {prevRoom} in the Real World first!";
                            }
                            else
                            {
                                lockMessage = $"{roomDisplayName} trial is locked!\nConquer Room {prevRoom} trial in the Alterworld first!";
                            }
                        }
                        else
                        {
                            int readCount = GameProgression.GetPlaquesReadCount(roomNum);
                            lockMessage = $"{roomDisplayName} trial is locked!\nVisit Real World Room {roomNum} and review all 5 plaques first! ({readCount}/5 studied)";
                        }
                    }
                }
                else
                {
                    int prevRoom = roomNum - 1;
                    lockMessage = $"{roomDisplayName} is currently locked!\nRestore all artifacts to Room {prevRoom} in the Real World first!";
                }

                NotificationBannerUI.Show(
                    "DOOR LOCKED",
                    lockMessage,
                    3.5f,
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
