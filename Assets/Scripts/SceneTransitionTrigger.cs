using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopDownGame
{
    /// <summary>
    /// Attach to doorway/passageway triggers connecting the central Lobby and the Left/Right Hallways.
    /// When the player walks into the trigger, transitions seamlessly to the destination scene
    /// and positions the player at the target entrance.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [Header("Target Destination")]
        [Tooltip("The scene to load (e.g. LeftHallway, RightHallway, Lobby)")]
        [SerializeField] private string targetSceneName = "Lobby";

        [Tooltip("Position where player should appear in the target scene")]
        [SerializeField] private Vector2 targetSpawnPosition = Vector2.zero;

        [Tooltip("Facing direction upon appearing in the target scene")]
        [SerializeField] private CharacterAnimator2D.FacingDirection targetFacing = CharacterAnimator2D.FacingDirection.Down;

        [Header("Cooldown")]
        [Tooltip("Grace period in seconds after scene load before this trigger can activate")]
        [SerializeField] private float entryCooldown = 0.5f;

        private bool _canTransition = false;
        private bool _isTransitioning = false;

        private void Start()
        {
            StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            _canTransition = false;
            yield return new WaitForSeconds(entryCooldown);
            _canTransition = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryTransition(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryTransition(other);
        }

        private void TryTransition(Collider2D other)
        {
            if (!_canTransition || _isTransitioning) return;
            if (!other.CompareTag("Player")) return;

            _isTransitioning = true;
            Debug.Log($"[SceneTransition] Transitioning from {SceneManager.GetActiveScene().name} to {targetSceneName} at spawn {targetSpawnPosition}");

            RoomTransitionManager.ReturnSceneName = SceneManager.GetActiveScene().name;
            RoomTransitionManager.ReturnPosition = targetSpawnPosition;
            RoomTransitionManager.HasReturnPosition = true;
            RoomTransitionManager.ReturnFacing = targetFacing;
            RoomTransitionManager.HasReturnFacing = true;

            SceneManager.LoadScene(targetSceneName);
        }

        public void SetConfig(string sceneName, Vector2 spawnPos, CharacterAnimator2D.FacingDirection facing)
        {
            targetSceneName = sceneName;
            targetSpawnPosition = spawnPos;
            targetFacing = facing;
        }
    }
}
