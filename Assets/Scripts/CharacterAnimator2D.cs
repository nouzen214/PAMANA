using UnityEngine;

namespace TopDownGame
{
    public class CharacterAnimator2D : MonoBehaviour
    {
        public enum FacingDirection
        {
            Down,
            Up,
            SideLeft,
            SideRight
        }

        [System.Serializable]
        public class CharacterData
        {
            public string characterName;
            public Sprite idleDown;
            public Sprite idleUp;
            public Sprite idleSide;
            public Sprite[] walkDown;
            public Sprite[] walkUp;
            public Sprite[] walkSide;
        }

        [Header("Character Profiles")]
        public CharacterData boyData = new CharacterData();
        public CharacterData girlData = new CharacterData();

        [Header("Animation Settings")]
        [SerializeField] private float walkFrameRate = 9f;
        [SerializeField] private bool enableWalkBobbing = true;
        [SerializeField] private float bobHeight = 0.05f;
        [SerializeField] private float bobSpeed = 16f;

        [Header("Cached References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private Rigidbody2D rb;

        private CharacterData _activeData;
        public bool IsGirlActive => _activeData != null ? (_activeData == girlData) : string.Equals(PlayerPrefs.GetString("SelectedCharacter", "Boy"), "Girl", System.StringComparison.OrdinalIgnoreCase);
        private FacingDirection _currentFacing = FacingDirection.Down;
        private float _frameTimer;
        private int _frameIndex;
        private bool _wasMoving;
        private float _bobTimer;
        private Vector3 _initialSpriteLocalPos;

        private void Awake()
        {
            EnsureReferences();
            if (spriteRenderer != null)
            {
                _initialSpriteLocalPos = spriteRenderer.transform.localPosition;
            }
            ApplyCharacterSelection();
        }

        private void OnValidate()
        {
            EnsureReferences();
        }

        public void EnsureReferences()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (inputHandler == null) inputHandler = GetComponent<PlayerInputHandler>();
            if (rb == null) rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            EnsureReferences();
            if (spriteRenderer != null)
            {
                _initialSpriteLocalPos = spriteRenderer.transform.localPosition;
            }
            ApplyCharacterSelection();
        }

        public void ApplyCharacterSelection()
        {
            EnsureReferences();
            string chosen = PlayerPrefs.GetString("SelectedCharacter", "Boy");
            _activeData = (string.Equals(chosen, "Girl", System.StringComparison.OrdinalIgnoreCase) && girlData != null && girlData.walkSide != null && girlData.walkSide.Length > 0) ? girlData : boyData;

            // Default initial state: Facing front idle
            _currentFacing = FacingDirection.Down;
            SetIdlePose();
        }

        public void SetFacing(FacingDirection direction)
        {
            EnsureReferences();
            _currentFacing = direction;
            SetIdlePose();
        }

        private void Update()
        {
            EnsureReferences();
            if (_activeData == null)
            {
                ApplyCharacterSelection();
            }
            if (spriteRenderer == null || _activeData == null) return;

            Vector2 moveInput = inputHandler != null ? inputHandler.MoveInput : Vector2.zero;

            // Fallback: If inputHandler hasn't updated or input is zero but physics body is moving, use linearVelocity
            if (moveInput.sqrMagnitude < 0.01f && rb != null && rb.linearVelocity.sqrMagnitude > 0.05f)
            {
                moveInput = rb.linearVelocity.normalized;
            }

            bool isMoving = moveInput.sqrMagnitude > 0.01f;

            if (isMoving)
            {
                _wasMoving = true;
                UpdateWalkingAnimation(moveInput);
                UpdateBobbing(true);
            }
            else
            {
                if (_wasMoving)
                {
                    _wasMoving = false;
                    _frameTimer = 0f;
                    _frameIndex = 0;
                    SetIdlePose();
                }
                UpdateBobbing(false);
            }
        }

        private void UpdateWalkingAnimation(Vector2 input)
        {
            // Advance frame timer
            _frameTimer += Time.deltaTime;
            float interval = 1f / Mathf.Max(1f, walkFrameRate);
            while (_frameTimer >= interval)
            {
                _frameTimer -= interval;
                _frameIndex++;
            }

            bool isGirl = string.Equals(_activeData.characterName, "Girl", System.StringComparison.OrdinalIgnoreCase);

            // Determine primary direction
            if (Mathf.Abs(input.x) >= Mathf.Abs(input.y) && Mathf.Abs(input.x) > 0.01f)
            {
                if (input.x > 0.01f)
                {
                    // Moving Right
                    _currentFacing = FacingDirection.SideRight;
                    spriteRenderer.flipX = false;

                    if (_activeData.walkSide != null && _activeData.walkSide.Length >= 6)
                    {
                        // 4-frame walk cycle using dedicated right-facing frames
                        // Stride -> Passing -> Push -> Passing
                        int[] rightSeq = isGirl ? new[] { 3, 4, 5, 4 } : new[] { 5, 4, 3, 4 };
                        int idx = rightSeq[_frameIndex % rightSeq.Length];
                        if (idx < _activeData.walkSide.Length && _activeData.walkSide[idx] != null)
                        {
                            spriteRenderer.sprite = _activeData.walkSide[idx];
                        }
                    }
                    else if (_activeData.walkSide != null && _activeData.walkSide.Length > 0)
                    {
                        int idx = _frameIndex % _activeData.walkSide.Length;
                        spriteRenderer.sprite = _activeData.walkSide[idx];
                    }
                }
                else
                {
                    // Moving Left
                    _currentFacing = FacingDirection.SideLeft;

                    if (_activeData.walkSide != null && _activeData.walkSide.Length >= 6)
                    {
                        // 4-frame walk cycle using dedicated left-facing frames
                        // Stride -> Passing -> Push -> Passing
                        spriteRenderer.flipX = false; // Native left-facing frames
                        int[] leftSeq = isGirl ? new[] { 2, 1, 0, 1 } : new[] { 0, 1, 2, 1 };
                        int idx = leftSeq[_frameIndex % leftSeq.Length];
                        if (idx < _activeData.walkSide.Length && _activeData.walkSide[idx] != null)
                        {
                            spriteRenderer.sprite = _activeData.walkSide[idx];
                        }
                    }
                    else if (_activeData.walkSide != null && _activeData.walkSide.Length > 0)
                    {
                        spriteRenderer.flipX = true;
                        int idx = _frameIndex % _activeData.walkSide.Length;
                        spriteRenderer.sprite = _activeData.walkSide[idx];
                    }
                }
            }
            else if (input.y > 0.05f)
            {
                // Moving Up (Back view)
                _currentFacing = FacingDirection.Up;
                spriteRenderer.flipX = false;

                if (_activeData.walkUp != null && _activeData.walkUp.Length >= 3)
                {
                    // 4-frame alternating cycle: Left Step (1) -> Mid (0) -> Right Step (2) -> Mid (0)
                    int[] upSeq = { 1, 0, 2, 0 };
                    int idx = upSeq[_frameIndex % upSeq.Length];
                    if (idx < _activeData.walkUp.Length && _activeData.walkUp[idx] != null)
                    {
                        spriteRenderer.sprite = _activeData.walkUp[idx];
                    }
                }
                else if (_activeData.walkUp != null && _activeData.walkUp.Length > 0)
                {
                    int idx = _frameIndex % _activeData.walkUp.Length;
                    spriteRenderer.sprite = _activeData.walkUp[idx];
                }
            }
            else if (input.y < -0.05f)
            {
                // Moving Down (Front view)
                _currentFacing = FacingDirection.Down;
                spriteRenderer.flipX = false;

                if (_activeData.walkDown != null && _activeData.walkDown.Length >= 3)
                {
                    // 4-frame alternating cycle: Left Step (1) -> Mid (0) -> Right Step (2) -> Mid (0)
                    int[] downSeq = { 1, 0, 2, 0 };
                    int idx = downSeq[_frameIndex % downSeq.Length];
                    if (idx < _activeData.walkDown.Length && _activeData.walkDown[idx] != null)
                    {
                        spriteRenderer.sprite = _activeData.walkDown[idx];
                    }
                }
                else if (_activeData.walkDown != null && _activeData.walkDown.Length > 0)
                {
                    int idx = _frameIndex % _activeData.walkDown.Length;
                    spriteRenderer.sprite = _activeData.walkDown[idx];
                }
            }
        }

        private void UpdateBobbing(bool isMoving)
        {
            if (!enableWalkBobbing || spriteRenderer == null) return;

            // CRITICAL: Only bob if SpriteRenderer is on a separate child visual object.
            // NEVER overwrite or pin the root Player transform position, which belongs to physics!
            if (spriteRenderer.transform == transform) return;

            if (isMoving)
            {
                _bobTimer += Time.deltaTime * bobSpeed;
                float offset = Mathf.Abs(Mathf.Sin(_bobTimer)) * bobHeight;
                spriteRenderer.transform.localPosition = new Vector3(
                    _initialSpriteLocalPos.x,
                    _initialSpriteLocalPos.y + offset,
                    _initialSpriteLocalPos.z
                );
            }
            else
            {
                _bobTimer = 0f;
                spriteRenderer.transform.localPosition = Vector3.MoveTowards(
                    spriteRenderer.transform.localPosition,
                    _initialSpriteLocalPos,
                    bobSpeed * Time.deltaTime
                );
            }
        }

        public void SetIdlePose()
        {
            EnsureReferences();
            if (_activeData == null || spriteRenderer == null) return;

            bool isGirl = string.Equals(_activeData.characterName, "Girl", System.StringComparison.OrdinalIgnoreCase);

            switch (_currentFacing)
            {
                case FacingDirection.SideRight:
                    spriteRenderer.flipX = false;
                    if (_activeData.walkSide != null && _activeData.walkSide.Length >= 6)
                    {
                        // Right-facing idle pose
                        int rightIdleIdx = isGirl ? 5 : 3;
                        spriteRenderer.sprite = _activeData.walkSide[rightIdleIdx];
                    }
                    else if (_activeData.idleSide != null)
                    {
                        spriteRenderer.sprite = _activeData.idleSide;
                    }
                    break;

                case FacingDirection.SideLeft:
                    if (_activeData.walkSide != null && _activeData.walkSide.Length >= 6)
                    {
                        // Dedicated left-facing idle pose
                        spriteRenderer.flipX = false;
                        int leftIdleIdx = isGirl ? 0 : 2;
                        spriteRenderer.sprite = _activeData.walkSide[leftIdleIdx];
                    }
                    else if (_activeData.idleSide != null)
                    {
                        spriteRenderer.sprite = _activeData.idleSide;
                        spriteRenderer.flipX = true;
                    }
                    break;

                case FacingDirection.Up:
                    spriteRenderer.flipX = false;
                    if (_activeData.idleUp != null)
                    {
                        spriteRenderer.sprite = _activeData.idleUp;
                    }
                    else if (_activeData.walkUp != null && _activeData.walkUp.Length > 0)
                    {
                        spriteRenderer.sprite = _activeData.walkUp[0];
                    }
                    break;

                case FacingDirection.Down:
                default:
                    spriteRenderer.flipX = false;
                    if (_activeData.idleDown != null)
                    {
                        spriteRenderer.sprite = _activeData.idleDown;
                    }
                    else if (_activeData.walkDown != null && _activeData.walkDown.Length > 0)
                    {
                        spriteRenderer.sprite = _activeData.walkDown[0];
                    }
                    break;
            }
        }
    }
}

