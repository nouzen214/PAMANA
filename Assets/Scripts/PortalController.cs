using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace TopDownGame
{
    [RequireComponent(typeof(Collider2D))]
    public class PortalController : MonoBehaviour
    {
        [Header("Activation Conditions")]
        [Tooltip("Require player to be moving upwards (WASD W or Joystick Up) to enter")]
        [SerializeField] private bool requireUpwardMovement = true;

        [Header("Scene Transition Destination")]
        [Tooltip("Target scene to load upon teleporting (e.g. AltLobby, Lobby)")]
        [SerializeField] private string targetSceneName = "AltLobby";

        [Tooltip("Spawn position in the target scene")]
        [SerializeField] private Vector2 targetSpawnPosition = new Vector2(0f, 1.8f);

        [Tooltip("Facing direction upon appearing in the target scene")]
        [SerializeField] private CharacterAnimator2D.FacingDirection targetFacing = CharacterAnimator2D.FacingDirection.Down;

        [Header("Progression Condition")]
        [Tooltip("If true and target is AltLobby, this portal is only enabled when GameProgression.IsPortalUnlocked is true.")]
        [SerializeField] private bool requireProgressionUnlock = true;

        [Header("Transition & Delay Settings")]
        [Tooltip("Duration of the suction and spin into the vortex")]
        [SerializeField] private float suctionDuration = 0.8f;
        [SerializeField] private float spinRotations = 2f;

        [Tooltip("Screen fade-out duration")]
        [SerializeField] private float fadeDuration = 0.5f;

        [Tooltip("Atmospheric holding delay in the realm void before loading the destination")]
        [SerializeField] private float transitionDelay = 0.45f;

        [Tooltip("Cooldown period in seconds after arrival before this portal can trigger")]
        [SerializeField] private float entryCooldown = 2.0f;

        [Tooltip("Screen fade-in duration upon appearing in the new world")]
        [SerializeField] private float arrivalFadeDuration = 0.6f;

        [Header("Events")]
        public UnityEvent onPortalEntered;
        public UnityEvent onTeleportComplete;

        // Static cooldown to prevent any portal triggering immediately after scene load
        private static float _globalTeleportCooldownUntil = 0f;

        private bool _isTeleporting = false;
        private bool _canActivate = false;
        private bool _hasReleasedWSinceArrival = true;

        public void SetConfig(string sceneName, Vector2 spawnPos, CharacterAnimator2D.FacingDirection facing)
        {
            targetSceneName = sceneName;
            targetSpawnPosition = spawnPos;
            targetFacing = facing;
        }

        private void Awake()
        {
            EnsureSideWalls();
            GameProgression.OnPortalUnlocked += HandleProgressionPortalUnlocked;
        }

        private void OnDestroy()
        {
            GameProgression.OnPortalUnlocked -= HandleProgressionPortalUnlocked;
        }

        private void HandleProgressionPortalUnlocked()
        {
            if (requireProgressionUnlock && targetSceneName == "AltLobby")
            {
                ForceEnableActivation();
            }
        }

        private void OnEnable()
        {
            if (requireProgressionUnlock && targetSceneName == "AltLobby")
            {
                bool unlocked = GameProgression.IsPortalUnlocked;
                if (!unlocked)
                {
                    _canActivate = false;
                    return;
                }
            }

            bool arrivedFromPortal = RoomTransitionManager.IsPortalTransition || ScreenFader.IsFadedOut;
            if (arrivedFromPortal)
            {
                RoomTransitionManager.IsPortalTransition = false;
                StartCoroutine(ArrivalRoutine());
            }
            else
            {
                ForceEnableActivation();
            }
        }

        public void ForceEnableActivation()
        {
            _canActivate = true;
            _hasReleasedWSinceArrival = true;
            _isTeleporting = false;

            // If player is already touching/inside the portal trigger, activate immediately
            var col = GetComponent<Collider2D>();
            if (col != null && col.enabled && gameObject.activeInHierarchy)
            {
                var contactFilter = new ContactFilter2D();
                contactFilter.NoFilter();
                var results = new System.Collections.Generic.List<Collider2D>();
                if (col.Overlap(contactFilter, results) > 0)
                {
                    foreach (var hit in results)
                    {
                        if (hit != null && hit.CompareTag("Player"))
                        {
                            TryActivatePortal(hit);
                            break;
                        }
                    }
                }
            }
        }

        public void EnsureSideWalls()
        {
            var walls = GameObject.Find("RoomWalls");
            Transform parent = walls != null ? walls.transform : null;

            var left = GameObject.Find("Lobby_PortalWall_Left");
            if (left == null)
            {
                left = new GameObject("Lobby_PortalWall_Left");
                if (parent != null) left.transform.SetParent(parent, false);
                left.transform.position = new Vector3(-1.225f, 3.275f, 0f);
                var col = left.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1.05f, 1.95f);
                col.isTrigger = false;
            }

            var right = GameObject.Find("Lobby_PortalWall_Right");
            if (right == null)
            {
                right = new GameObject("Lobby_PortalWall_Right");
                if (parent != null) right.transform.SetParent(parent, false);
                right.transform.position = new Vector3(1.225f, 3.275f, 0f);
                var col = right.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1.05f, 1.95f);
                col.isTrigger = false;
            }

            var backWall = GameObject.Find("Lobby_PortalBackWall");
            if (backWall != null)
            {
                var col = backWall.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    backWall.transform.position = new Vector3(0f, 4.75f, 0f);
                    col.size = new Vector2(3.5f, 2.0f);
                }
            }
        }

        private void Start()
        {
            if (requireProgressionUnlock && targetSceneName == "AltLobby")
            {
                bool unlocked = GameProgression.IsPortalUnlocked;
                if (!unlocked)
                {
                    gameObject.SetActive(false);
                    _canActivate = false;
                    return;
                }
            }

            if (!_canActivate && !_isTeleporting)
            {
                bool arrivedFromPortal = RoomTransitionManager.IsPortalTransition || ScreenFader.IsFadedOut;
                if (arrivedFromPortal)
                {
                    RoomTransitionManager.IsPortalTransition = false;
                    StartCoroutine(ArrivalRoutine());
                }
                else
                {
                    ForceEnableActivation();
                }
            }
        }

        private IEnumerator ArrivalRoutine()
        {
            _canActivate = false;
            _hasReleasedWSinceArrival = false;

            // Find player
            GameObject player = GameObject.FindWithTag("Player");
            PlayerMovement movement = player != null ? player.GetComponent<PlayerMovement>() : null;
            Rigidbody2D rb = player != null ? player.GetComponent<Rigidbody2D>() : null;

            if (movement != null) movement.enabled = false;
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // Small initial pause for scene and camera to position
            yield return new WaitForSeconds(0.1f);

            // Smooth fade-in
            ScreenFader.Instance.FadeIn(arrivalFadeDuration);

            // Emergence animation: gently lerp scale if player exists
            if (player != null)
            {
                Vector3 targetScale = Vector3.one;
                Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
                float elapsed = 0f;
                while (elapsed < arrivalFadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / arrivalFadeDuration);
                    player.transform.localScale = Vector3.Lerp(startScale, targetScale, t * (2f - t));
                    yield return null;
                }
                player.transform.localScale = targetScale;
            }
            else
            {
                yield return new WaitForSeconds(arrivalFadeDuration);
            }

            // Restore player movement
            if (movement != null) movement.enabled = true;

            // Keep portal inactive for the remaining entryCooldown
            float remainingCooldown = Mathf.Max(0.5f, entryCooldown - arrivalFadeDuration);
            yield return new WaitForSeconds(remainingCooldown);

            _canActivate = true;
        }

        private void Update()
        {
            if (!_hasReleasedWSinceArrival)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    var inputHandler = player.GetComponent<PlayerInputHandler>();
                    // Only unlock when player has released the 'W' key (MoveInput.y <= 0.1f)
                    if (inputHandler != null && inputHandler.MoveInput.y <= 0.1f)
                    {
                        _hasReleasedWSinceArrival = true;
                    }
                }
                else
                {
                    _hasReleasedWSinceArrival = true;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryActivatePortal(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryActivatePortal(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _hasReleasedWSinceArrival = true;
            }
        }

        private void TryActivatePortal(Collider2D other)
        {
            if (_isTeleporting) return;
            if (!_canActivate) return;
            if (Time.time < _globalTeleportCooldownUntil) return;
            if (!_hasReleasedWSinceArrival) return;
            if (requireProgressionUnlock && targetSceneName == "AltLobby" && !GameProgression.IsPortalUnlocked) return;

            if (other.CompareTag("Player"))
            {
                var inputHandler = other.GetComponent<PlayerInputHandler>();
                bool isMovingUp = inputHandler == null || inputHandler.MoveInput.y > 0.1f;
                bool isInsidePortal = Vector2.Distance(other.transform.position, transform.position) < 0.85f;

                if (!requireUpwardMovement || isMovingUp || isInsidePortal)
                {
                    StartCoroutine(TeleportRoutine(other.gameObject));
                }
            }
        }

        private IEnumerator TeleportRoutine(GameObject player)
        {
            _isTeleporting = true;
            _globalTeleportCooldownUntil = Time.time + 10f; // Lock until arrival and beyond
            onPortalEntered?.Invoke();

            // Disable player control during teleport
            var movement = player.GetComponent<PlayerMovement>();
            var rb = player.GetComponent<Rigidbody2D>();
            var col = player.GetComponent<Collider2D>();

            if (movement != null) movement.enabled = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
            if (col != null) col.enabled = false;

            Vector3 startPos = player.transform.position;
            Vector3 targetPos = transform.position;
            Vector3 startScale = player.transform.localScale;

            // Start screen fade-out to dark void
            ScreenFader.Instance.FadeOut(fadeDuration, new Color(0.03f, 0.015f, 0.06f, 1f));

            float elapsed = 0f;
            while (elapsed < suctionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / suctionDuration);
                float easeIn = t * t;

                // Move towards portal center
                player.transform.position = Vector3.Lerp(startPos, targetPos, easeIn);

                // Spin and shrink into vortex
                player.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, easeIn);
                player.transform.Rotate(0f, 0f, 360f * spinRotations * Time.deltaTime / suctionDuration);

                yield return null;
            }

            player.transform.localScale = Vector3.zero;
            onTeleportComplete?.Invoke();
            Debug.Log($"[PortalController] Entering portal transition to {targetSceneName}...");

            // Atmospheric realm void transition delay
            yield return new WaitForSeconds(transitionDelay);

            if (!string.IsNullOrEmpty(targetSceneName))
            {
                RoomTransitionManager.IsPortalTransition = true;
                RoomTransitionManager.ReturnSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                RoomTransitionManager.ReturnPosition = targetSpawnPosition;
                RoomTransitionManager.HasReturnPosition = true;
                RoomTransitionManager.ReturnFacing = targetFacing;
                RoomTransitionManager.HasReturnFacing = true;

                _globalTeleportCooldownUntil = Time.time + 3.0f;

                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                // Fallback local respawn
                player.transform.position = new Vector3(targetSpawnPosition.x, targetSpawnPosition.y, 0f);
                player.transform.rotation = Quaternion.identity;
                player.transform.localScale = startScale;

                if (rb != null) rb.simulated = true;
                if (col != null) col.enabled = true;
                if (movement != null) movement.enabled = true;

                ScreenFader.Instance.FadeIn(0.5f);
                _isTeleporting = false;
            }
        }
    }
}
