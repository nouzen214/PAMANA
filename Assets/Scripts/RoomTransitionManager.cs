using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopDownGame
{
    /// <summary>
    /// Static coordinator for transitioning between the Hallways/Lobby and the 12 Rooms or Battle Scenes,
    /// remembering which door the player entered from so they return to that exact hallway doorway.
    /// Persists return coordinates across logouts, page reloads, and scene re-entries.
    /// </summary>
    public static class RoomTransitionManager
    {
        public static string ReturnDoorName { get; set; } = null;
        public static Vector2 ReturnPosition { get; set; } = Vector2.zero;
        public static bool HasReturnPosition { get; set; } = false;
        public static string ReturnSceneName { get; set; } = "Lobby";
        public static CharacterAnimator2D.FacingDirection ReturnFacing { get; set; } = CharacterAnimator2D.FacingDirection.Down;
        public static bool HasReturnFacing { get; set; } = false;
        public static bool IsPortalTransition { get; set; } = false;
        public static string CurrentRoomDisplayName { get; set; } = "Room 1";
        public static int CurrentRoomNumber { get; set; } = 1;

        public static void EnterRoom(string roomSceneName, string doorName, Vector2 returnPos)
        {
            ReturnSceneName = SceneManager.GetActiveScene().name;
            ReturnDoorName = doorName;
            ReturnPosition = returnPos;
            HasReturnPosition = true;

            // Facing depends on whether it's a top door (face down into hallway) or bottom door (face up)
            if (!string.IsNullOrEmpty(doorName) && doorName.Contains("Bottom"))
            {
                ReturnFacing = CharacterAnimator2D.FacingDirection.Up;
            }
            else
            {
                ReturnFacing = CharacterAnimator2D.FacingDirection.Down;
            }
            HasReturnFacing = true;

            SaveToPrefs();

            Debug.Log($"[RoomTransition] Entering {roomSceneName} from {ReturnSceneName} door '{doorName}' (Return pos: {returnPos})");
            SceneManager.LoadScene(roomSceneName);
        }

        public static void ExitToLobby()
        {
            EnsureRoomReturnInfo(SceneManager.GetActiveScene().name);

            string target = string.IsNullOrEmpty(ReturnSceneName) ? "Lobby" : ReturnSceneName;
            Debug.Log($"[RoomTransition] Exiting back to {target}. Return door: '{ReturnDoorName}', ReturnPos: {ReturnPosition}, HasReturnPosition: {HasReturnPosition}");
            SceneManager.LoadScene(target);
        }

        public static void SaveToPrefs()
        {
            PlayerPrefs.SetString("RoomTransition_ReturnScene", ReturnSceneName ?? "");
            PlayerPrefs.SetString("RoomTransition_ReturnDoor", ReturnDoorName ?? "");
            PlayerPrefs.SetFloat("RoomTransition_ReturnPosX", ReturnPosition.x);
            PlayerPrefs.SetFloat("RoomTransition_ReturnPosY", ReturnPosition.y);
            PlayerPrefs.SetInt("RoomTransition_HasReturnPos", HasReturnPosition ? 1 : 0);
            PlayerPrefs.SetInt("RoomTransition_ReturnFacing", (int)ReturnFacing);
            PlayerPrefs.SetInt("RoomTransition_HasReturnFacing", HasReturnFacing ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void LoadFromPrefs()
        {
            if (PlayerPrefs.GetInt("RoomTransition_HasReturnPos", 0) == 1)
            {
                string savedScene = PlayerPrefs.GetString("RoomTransition_ReturnScene", "");
                if (!string.IsNullOrEmpty(savedScene))
                {
                    ReturnSceneName = savedScene;
                    ReturnDoorName = PlayerPrefs.GetString("RoomTransition_ReturnDoor", "");
                    ReturnPosition = new Vector2(
                        PlayerPrefs.GetFloat("RoomTransition_ReturnPosX", 0f),
                        PlayerPrefs.GetFloat("RoomTransition_ReturnPosY", 0f)
                    );
                    HasReturnPosition = true;

                    if (PlayerPrefs.GetInt("RoomTransition_HasReturnFacing", 0) == 1)
                    {
                        ReturnFacing = (CharacterAnimator2D.FacingDirection)PlayerPrefs.GetInt("RoomTransition_ReturnFacing", 0);
                        HasReturnFacing = true;
                    }
                }
            }
        }

        /// <summary>
        /// Guarantees that the player will exit to their correct hallway and door location,
        /// even if they logged in or reloaded directly into a Room or BattleScene.
        /// </summary>
        public static void EnsureRoomReturnInfo(string currentSceneName = null)
        {
            if (string.IsNullOrEmpty(currentSceneName))
            {
                currentSceneName = SceneManager.GetActiveScene().name;
            }

            // 1. If in-memory state already has a specific hallway return target, we're set
            if (HasReturnPosition && !string.IsNullOrEmpty(ReturnSceneName) && ReturnSceneName != "Lobby")
            {
                return;
            }

            // 2. Check PlayerPrefs for persisted return info
            LoadFromPrefs();
            if (HasReturnPosition && !string.IsNullOrEmpty(ReturnSceneName) && ReturnSceneName != "Lobby")
            {
                return;
            }

            // 3. Fallback: Automatically deduce the exact hallway and doorway coordinates from room name
            if (TryGetDefaultReturnForRoom(currentSceneName, out string deducedScene, out Vector2 deducedPos, out string deducedDoor))
            {
                ReturnSceneName = deducedScene;
                ReturnPosition = deducedPos;
                ReturnDoorName = deducedDoor;
                HasReturnPosition = true;

                if (!string.IsNullOrEmpty(deducedDoor) && deducedDoor.Contains("Bottom"))
                {
                    ReturnFacing = CharacterAnimator2D.FacingDirection.Up;
                }
                else
                {
                    ReturnFacing = CharacterAnimator2D.FacingDirection.Down;
                }
                HasReturnFacing = true;

                SaveToPrefs();
            }
        }

        /// <summary>
        /// Maps each room scene or battle trial to its corresponding hallway and doorway coordinates.
        /// </summary>
        public static bool TryGetDefaultReturnForRoom(string roomSceneName, out string returnScene, out Vector2 returnPos, out string doorName)
        {
            returnScene = "LeftHallway";
            returnPos = Vector2.zero;
            doorName = "";

            if (string.IsNullOrEmpty(roomSceneName)) return false;

            switch (roomSceneName)
            {
                // Normal World Rooms (Room_1 to Room_12)
                case "Room_1":
                    returnScene = "LeftHallway";
                    returnPos = new Vector2(-7.94f, 1.56f);
                    doorName = "LeftHall_TopDoor_1";
                    return true;
                case "Room_2":
                    returnScene = "LeftHallway";
                    returnPos = new Vector2(-1.19f, 1.06f);
                    doorName = "LeftHall_TopDoor_2";
                    return true;
                case "Room_3":
                    returnScene = "LeftHallway";
                    returnPos = new Vector2(5.18f, 0.84f);
                    doorName = "LeftHall_TopDoor_3";
                    return true;
                case "Room_4":
                    returnScene = "LeftHallway";
                    returnPos = new Vector2(-7.72f, -1.46f);
                    doorName = "LeftHall_BottomDoor_1";
                    return true;
                case "Room_5":
                    returnScene = "LeftHallway";
                    returnPos = new Vector2(-1.32f, -1.46f);
                    doorName = "LeftHall_BottomDoor_2";
                    return true;
                case "Room_6":
                    returnScene = "LeftHallway";
                    returnPos = new Vector2(5.16f, -1.46f);
                    doorName = "LeftHall_BottomDoor_3";
                    return true;

                case "Room_7":
                    returnScene = "RightHallway";
                    returnPos = new Vector2(-5.13f, 0.88f);
                    doorName = "RightHall_TopDoor_1";
                    return true;
                case "Room_8":
                    returnScene = "RightHallway";
                    returnPos = new Vector2(1.25f, 0.82f);
                    doorName = "RightHall_TopDoor_2";
                    return true;
                case "Room_9":
                    returnScene = "RightHallway";
                    returnPos = new Vector2(7.75f, 1.21f);
                    doorName = "RightHall_TopDoor_3";
                    return true;
                case "Room_10":
                    returnScene = "RightHallway";
                    returnPos = new Vector2(-5.16f, -1.37f);
                    doorName = "RightHall_BottomDoor_1";
                    return true;
                case "Room_11":
                    returnScene = "RightHallway";
                    returnPos = new Vector2(1.19f, -1.43f);
                    doorName = "RightHall_BottomDoor_2";
                    return true;
                case "Room_12":
                    returnScene = "RightHallway";
                    returnPos = new Vector2(7.69f, -1.46f);
                    doorName = "RightHall_BottomDoor_3";
                    return true;

                // Alterworld Battle Trials
                case "BattleScene": // Room 1 Battle Trial
                    returnScene = "AltLeftHallway";
                    returnPos = new Vector2(-7.94f, 1.56f);
                    doorName = "LeftHall_TopDoor_1";
                    return true;
                case "BattleScene_2":
                    returnScene = "AltLeftHallway";
                    returnPos = new Vector2(-1.19f, 1.06f);
                    doorName = "LeftHall_TopDoor_2";
                    return true;
                case "BattleScene_3":
                    returnScene = "AltLeftHallway";
                    returnPos = new Vector2(5.18f, 0.84f);
                    doorName = "LeftHall_TopDoor_3";
                    return true;
                case "BattleScene_4":
                    returnScene = "AltLeftHallway";
                    returnPos = new Vector2(-7.72f, -1.46f);
                    doorName = "LeftHall_BottomDoor_1";
                    return true;
                case "BattleScene_5":
                    returnScene = "AltLeftHallway";
                    returnPos = new Vector2(-1.32f, -1.46f);
                    doorName = "LeftHall_BottomDoor_2";
                    return true;
                case "BattleScene_6":
                    returnScene = "AltLeftHallway";
                    returnPos = new Vector2(5.16f, -1.46f);
                    doorName = "LeftHall_BottomDoor_3";
                    return true;

                case "BattleScene_7":
                    returnScene = "AltRightHallway";
                    returnPos = new Vector2(-5.13f, 0.88f);
                    doorName = "RightHall_TopDoor_1";
                    return true;
                case "BattleScene_8":
                    returnScene = "AltRightHallway";
                    returnPos = new Vector2(1.25f, 0.82f);
                    doorName = "RightHall_TopDoor_2";
                    return true;
                case "BattleScene_9":
                    returnScene = "AltRightHallway";
                    returnPos = new Vector2(7.75f, 1.21f);
                    doorName = "RightHall_TopDoor_3";
                    return true;
                case "BattleScene_10":
                    returnScene = "AltRightHallway";
                    returnPos = new Vector2(-5.16f, -1.37f);
                    doorName = "RightHall_BottomDoor_1";
                    return true;
                case "BattleScene_11":
                    returnScene = "AltRightHallway";
                    returnPos = new Vector2(1.19f, -1.43f);
                    doorName = "RightHall_BottomDoor_2";
                    return true;
                case "BattleScene_12":
                    returnScene = "AltRightHallway";
                    returnPos = new Vector2(7.69f, -1.46f);
                    doorName = "RightHall_BottomDoor_3";
                    return true;
            }

            return false;
        }

        public static void Reset()
        {
            ReturnDoorName = null;
            ReturnPosition = Vector2.zero;
            HasReturnPosition = false;
            ReturnSceneName = "Lobby";
            HasReturnFacing = false;
            IsPortalTransition = false;
            CurrentRoomDisplayName = "Room 1";
            CurrentRoomNumber = 1;

            PlayerPrefs.DeleteKey("RoomTransition_ReturnScene");
            PlayerPrefs.DeleteKey("RoomTransition_ReturnDoor");
            PlayerPrefs.DeleteKey("RoomTransition_ReturnPosX");
            PlayerPrefs.DeleteKey("RoomTransition_ReturnPosY");
            PlayerPrefs.DeleteKey("RoomTransition_HasReturnPos");
            PlayerPrefs.DeleteKey("RoomTransition_ReturnFacing");
            PlayerPrefs.DeleteKey("RoomTransition_HasReturnFacing");
            PlayerPrefs.Save();
        }
    }
}
