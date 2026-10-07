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

        public string TargetSceneName => targetSceneName;
        public string RoomDisplayName => roomDisplayName;

        public override void Interact()
        {
            EnterRoom();
        }

        public override string GetPromptText(string openText, string closeText)
        {
            return $"[E] Enter {roomDisplayName}";
        }

        public void EnterRoom()
        {
            Vector2 spawnPos = (Vector2)transform.position + returnOffset;
            RoomTransitionManager.EnterRoom(targetSceneName, gameObject.name, spawnPos);
        }

        public void SetConfig(string sceneName, string displayName, Vector2 offset)
        {
            targetSceneName = sceneName;
            roomDisplayName = displayName;
            returnOffset = offset;
        }
    }
}
