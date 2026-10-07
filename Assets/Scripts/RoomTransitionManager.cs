using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopDownGame
{
    /// <summary>
    /// Static coordinator for transitioning between the Lobby and the 12 Rooms,
    /// remembering which door the player entered from so they return to that exact door.
    /// </summary>
    public static class RoomTransitionManager
    {
        public static string ReturnDoorName { get; set; } = null;
        public static Vector2 ReturnPosition { get; set; } = Vector2.zero;
        public static bool HasReturnPosition { get; set; } = false;
        public static string ReturnSceneName { get; set; } = "Lobby";
        public static CharacterAnimator2D.FacingDirection ReturnFacing { get; set; } = CharacterAnimator2D.FacingDirection.Down;
        public static bool HasReturnFacing { get; set; } = false;

        public static void EnterRoom(string roomSceneName, string doorName, Vector2 returnPos)
        {
            ReturnSceneName = SceneManager.GetActiveScene().name;
            ReturnDoorName = doorName;
            ReturnPosition = returnPos;
            HasReturnPosition = true;

            Debug.Log($"[RoomTransition] Entering {roomSceneName} from {ReturnSceneName} door '{doorName}' (Return pos: {returnPos})");
            SceneManager.LoadScene(roomSceneName);
        }

        public static void ExitToLobby()
        {
            string target = string.IsNullOrEmpty(ReturnSceneName) ? "Lobby" : ReturnSceneName;
            Debug.Log($"[RoomTransition] Exiting back to {target}. Return door: '{ReturnDoorName}', HasReturnPosition: {HasReturnPosition}");
            SceneManager.LoadScene(target);
        }
    }
}
