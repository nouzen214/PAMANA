using UnityEngine;

namespace TopDownGame
{
    /// <summary>
    /// Attach to an object in the Lobby (e.g. on LobbyManager or Player).
    /// If returning from a Room scene, spawns the player directly in front of the door they entered.
    /// </summary>
    public class LobbySpawnHandler : MonoBehaviour
    {
        [Tooltip("Optional reference to player transform. If null, finds Player by tag.")]
        [SerializeField] private Transform playerTransform;

        private void Start()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;
            }

            if (RoomTransitionManager.HasReturnPosition && playerTransform != null)
            {
                Debug.Log($"[LobbySpawnHandler] Repositioning player to return position: {RoomTransitionManager.ReturnPosition}");
                playerTransform.position = new Vector3(
                    RoomTransitionManager.ReturnPosition.x,
                    RoomTransitionManager.ReturnPosition.y,
                    0f
                );

                // Align camera immediately
                var cam = Camera.main;
                if (cam != null)
                {
                    var camFollow = cam.GetComponent<CameraFollow2D>();
                    if (camFollow != null)
                    {
                        cam.transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, cam.transform.position.z);
                    }
                }

                if (RoomTransitionManager.HasReturnFacing)
                {
                    var anim = playerTransform.GetComponent<CharacterAnimator2D>();
                    if (anim != null)
                    {
                        anim.SetFacing(RoomTransitionManager.ReturnFacing);
                    }
                    RoomTransitionManager.HasReturnFacing = false;
                }

                // Consume the return position
                RoomTransitionManager.HasReturnPosition = false;
            }
        }
    }
}
