using Attributes;
using Consensus;
using ExtensionMethods;
using Fmod;
using PlayerLoopTimer;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Player {
    public class PlayerMovement : MonoBehaviour {
        #region ---Variables---

        #region ***---RequiredReferences---

        [RequiredReference, HideInInspector] public Rigidbody2D playerRigidbody;
        [RequiredReference, HideInInspector] public Animator animator;
        [RequiredReference] private BoxCollider2D _playerCollider;
        [RequiredReference] private PlayerScript _playerScript;

        #endregion

        #region ***---Serialized---

        [Header("Movement")] [Required] public AllInFavourConsensus playerCanMove;

        [Header("Walking")] [SerializeField] private float maxSpeed;

        [FormerlySerializedAs("walkingSpeedCurve")] [SerializeField]
        private AnimationCurve walkingSpeedCurveAnimator;

        [SerializeField] private AnimationCurve accelerationCurve;
        [SerializeField] private float moveAcceleration;

        [Header("Jump")] [SerializeField] private float jumpForce;
        [SerializeField] private AnimationCurve jumpForceCurve;
        [SerializeField] private float jumpMaxDuration;
        [SerializeField] private float minimumJumpTime;
        private float _currentJumpDuration;
        [SerializeField] private float jumpBufferTime = 0.1f;
        [SerializeField] private bool canJump;

        [Header("Falling")] [SerializeField] private float fallMultiplier;
        [SerializeField] private float maxFallVelocity;

        [Header("Coyote Time")] [Tooltip("Only gets applied in Start"), SerializeField]
        private float coyoteTime;

        [Header("Wall Jump / Wall Slide")] [SerializeField]
        private bool canWallJump = true;

        [SerializeField] private float wallSlideVelocity;

        [Tooltip("Only gets applied in Start"), SerializeField]
        private float wallJumpTime = 0.2f;

        [Header("Climbing")] [SerializeField] private float climbSpeed;
        public bool isClimbing;

        [Header("Dash")] [Tooltip("Only gets applied in Start"), SerializeField]
        private float dashTime;

        [SerializeField] private float dashSpeed;
        [SerializeField] private AnimationCurve dashSpeedCurve;
        public bool isDashing;

        [Header("GunJump")] [SerializeField] private float gunJumpForce;
        public bool hasFreeDash;
        public int killCharges;

        [Header("Ground Check")] [SerializeField]
        private float distanceToGround;

        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private float distanceToWall;
        [SerializeField] private float wallJumpYForce;
        public bool isGrounded;
        public bool isSolidGrounded;

        [Header("Ladder Check")] [SerializeField]
        private LayerMask whatIsLadder;

        [Header("Material")]
        [SerializeField] private Material playerMaterial;

        [Header("Debug Logger")] [SerializeField]
        private DebugLoggerSO debugLogger;

        #endregion

        #region ***---PrivateVariables---

        private Vector2 _move;
        private bool _wasGroundedLastFrame;

        private float _facingDirection = 1;

        private float directionalDashForce => dashSpeed * _facingDirection;

        public bool playerInputEnabled => playerCanMove.consensus;
        private readonly ScriptVotePair _scriptVotePair = new() { vote = true };

        private bool _isWallSliding;
        private bool _isWallJumping;
        private bool _isJumping;

        private CountdownTimer _dashTimer;
        private CountdownTimer _wallJumpTimer;
        private CountdownTimer _coyoteTimer;
        private CountdownTimer _jumpBufferTimer;

        private bool _isHoldingJump;

        private static readonly int AnimatorIsFalling = Animator.StringToHash("IsFalling");
        private static readonly int AnimatorIsJumping = Animator.StringToHash("IsJumping");
        private static readonly int AnimatorIsWalking = Animator.StringToHash("IsWalking");
        private static readonly int WalkSpeedMultiplier = Animator.StringToHash("WalkSpeedMultiplier");
        private static readonly int AnimatorIsDashing = Animator.StringToHash("IsDashing");
        private static readonly int AnimatorGunJump = Animator.StringToHash("GunJump");
        private static readonly int AnimatorIsWallSliding = Animator.StringToHash("IsWallSliding");
        private static readonly int AnimatorWallJump = Animator.StringToHash("WallJump");
        private static readonly int Ladder = Animator.StringToHash("Ladder");

        private static readonly int Boolean = Shader.PropertyToID("_Boolean");

        private float _lastTimeJumpPressed;

        private FmodManager fmodManager => FmodManager.Instance;

        #endregion

        #endregion

        private void Awake() {
            debugLogger.messageOrigin = this;
        }

        private void Start() {
            _playerScript = ServiceLocator.Get<PlayerScript>();

            InitializeDashTimer();
            InitializeWallJumpTimer();
            InitializeCoyoteTimer();
            InitializeJumpBufferTimer();
        }

        private void OnEnable() {
            playerCanMove.AddVote(_scriptVotePair);
        }

        private void OnDisable() {
            playerCanMove.DeleteVote(_scriptVotePair);
        }

        #region ---TimerInitialization---

        private void InitializeDashTimer() {
            _dashTimer = new CountdownTimer(dashTime);
            _dashTimer.OnBegin += () => {
                isDashing = true;
                animator.SetBool(AnimatorIsDashing, true);
            };
            _dashTimer.OnUpdate += () => {
                var xSpeed = directionalDashForce * dashSpeedCurve.Evaluate(_dashTimer.CurrentProgress);
                playerRigidbody.linearVelocity = new Vector2(xSpeed, 0);
                debugLogger.Log("Dashing | " + _dashTimer.CurrentProgress + " | " + _dashTimer.Duration);
            };
            _dashTimer.OnStop += () => {
                isDashing = false;
                animator.SetBool(AnimatorIsDashing, false);
            };
        }

        private void InitializeWallJumpTimer() {
            _wallJumpTimer = new CountdownTimer(wallJumpTime);
            _wallJumpTimer.OnBegin += () => { _isWallJumping = true; };
            _wallJumpTimer.OnComplete += () => { _isWallJumping = false; };
        }

        private void InitializeCoyoteTimer() {
            _coyoteTimer = new CountdownTimer(coyoteTime);
            _coyoteTimer.OnComplete += () => {
                canJump = false;
                debugLogger.Log("Coyote time finished");
            };
        }

        private void InitializeJumpBufferTimer() {
            _jumpBufferTimer = new CountdownTimer(jumpBufferTime);
            _jumpBufferTimer.OnUpdate += () => {
                if (canJump && isGrounded) {
                    Jump();

                    _jumpBufferTimer.StopTimer();
                }
            };
        }

        #endregion

        #region ---InputActions---

        public void OnMove(InputAction.CallbackContext context) {
            if (!playerInputEnabled && !_playerScript.isAttacking) {
                _move = Vector2.zero;
                return;
            }

            _move = context.ReadValue<Vector2>();

            if (_move.y < -0.5f && isGrounded) {
                var platform = PlatformCheck();
                if (platform != null) {
                    platform.DisablePlatformCollider();
                    _playerCollider.size = _playerCollider.size.With(x: 0.8f);
                    animator.SetBool(Ladder, true);
                }
            }
            else {
                _playerCollider.size = _playerCollider.size.With(x: 0.95f);
                animator.SetBool(Ladder, false);
            }
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.performed && playerInputEnabled) {
                isClimbing = false;
                _dashTimer.StopTimer();

                if (WallCheck() && canWallJump && !isGrounded) {
                    debugLogger.Log("Wall Jump Pressed");
                    playerRigidbody.linearVelocityY = wallJumpYForce;
                    playerRigidbody.linearVelocityX -= jumpForce * _facingDirection;

                    _wallJumpTimer.StartTimer();

                    animator.SetTrigger(AnimatorWallJump);
                }
                else if (canJump && isGrounded) {
                    Jump();
                    _isHoldingJump = true;
                }
                else if (killCharges > 0) {
                    debugLogger.Log("Gun Jump");
                    fmodManager.PlayOneShot("event:/Sfx/Player/Player_gunjump");
                    playerRigidbody.linearVelocityY = gunJumpForce;
                    isClimbing = false;
                    _lastTimeJumpPressed = Time.time + minimumJumpTime;
                    killCharges--;
                    animator.SetTrigger(AnimatorGunJump);
                }
                else {
                    _jumpBufferTimer.RestartTimer();
                }
            }
            else if (context.canceled || !playerInputEnabled) {
                debugLogger.Log("Jumped Released");
                _isHoldingJump = false;
            }
        }

        public void OnDash(InputAction.CallbackContext context) {
            if (!playerInputEnabled || !context.performed || isDashing) return;

            if (hasFreeDash) {
                hasFreeDash = false;
            }
            else if (killCharges > 0) {
                killCharges--;
            }
            else {
                _playerScript.Attack();
                return;
            }

            debugLogger.Log("Dashing");

            fmodManager.PlayOneShot("event:/Sfx/Player/Player_dash");

            isClimbing = false;

            _dashTimer.StartTimer();

            _playerScript.Attack();
        }

        #endregion

        #region ---UnityEventFunctions---

        private void Update() {
            isGrounded = GroundCheck() && !isDashing && !_isHoldingJump;
            isSolidGrounded = SolidGroundCheck();
            _isWallSliding = WallCheck() && !isGrounded && _move.Abs().x > 0;
            isClimbing = LadderCheck() && _move.y != 0 && !PlatformCheck();

            debugLogger.Log(
                $"IsGrounded: {isGrounded} | IsWallSliding: {_isWallSliding} | IsWallJumping: {_isWallJumping} | WallCheck: {WallCheck()}");

            animator.SetBool(AnimatorIsWallSliding, _isWallSliding);

            playerMaterial.SetInt(Boolean, killCharges > 0 ? 1 : 0);

            if (isGrounded) {
                canJump = true;
                hasFreeDash = true;
                _isJumping = false;
                killCharges = 0;
                _wasGroundedLastFrame = true;
                _coyoteTimer.StopTimer();
            }
            else if (_wasGroundedLastFrame && !_isJumping) {
                _coyoteTimer.StartTimer();
                _wasGroundedLastFrame = false;
            }

            animator.SetBool(AnimatorIsJumping, _isJumping);

            if (_isWallSliding) {
                _dashTimer.StopTimer();
                hasFreeDash = true;
            }

            if (isClimbing) {
                isGrounded = true;
            }
            else if (isSolidGrounded) {
                Vector2 potentialSpawnPosition = transform.position.WithOffset(x: -_facingDirection);

                if (!IsSpawnPositionValid(potentialSpawnPosition)) {
                    // If searchiterations is even, search to the right, if it is odd, search to the left. Every time it fails to find a valid spawn position, increase the search range.
                    potentialSpawnPosition = transform.position.WithOffset(x: _facingDirection);
                }

                if (IsSpawnPositionValid(potentialSpawnPosition)) {
                    _playerScript.lastGroundedPosition = potentialSpawnPosition;
                }
            }

            if (!isDashing && !_isWallJumping && playerInputEnabled) {
                CheckFacingDirection(_move);
            }
        }

        private void FixedUpdate() {
            if (playerRigidbody.linearVelocityY < -0.1f) {
                playerRigidbody.linearVelocityY *= fallMultiplier;
                playerRigidbody.linearVelocityY = Mathf.Clamp(playerRigidbody.linearVelocityY, -maxFallVelocity, 0);
                animator.SetBool(AnimatorIsFalling, true);
                // animator.SetBool(AnimatorIsJumping, false);
            }
            else {
                animator.SetBool(AnimatorIsFalling, false);
            }

            if (_isHoldingJump) {
                _currentJumpDuration += Time.fixedDeltaTime;
                playerRigidbody.linearVelocityY += jumpForceCurve.Evaluate(_currentJumpDuration / jumpMaxDuration);
            }
            else {
                _currentJumpDuration = 0;
            }

            if (!isGrounded && _lastTimeJumpPressed <= Time.time - 0.2f &&
                (_currentJumpDuration >= jumpMaxDuration || !_isHoldingJump) && !_isWallJumping) {
                playerRigidbody.linearVelocityY = Mathf.Clamp(playerRigidbody.linearVelocityY, -maxFallVelocity, 0);
            }

            if (!isClimbing) {
                playerRigidbody.gravityScale = 1;
                if (!isDashing && !_isWallJumping) {
                    if (isGrounded && _move.x == 0) {
                        playerRigidbody.linearVelocityX *= 0.75f;
                    }
                    else {
                        playerRigidbody.linearVelocityX *= 0.9f;
                    }

                    if (!playerInputEnabled) {
                        animator.SetBool(AnimatorIsWalking, false);
                        return;
                    }

                    animator.SetBool(Ladder, false);

                    var acceleration =
                        accelerationCurve.Evaluate(math.abs(playerRigidbody.linearVelocityX) / maxSpeed) *
                        moveAcceleration;
                    animator.SetBool(AnimatorIsWalking, _move != Vector2.zero);

                    if (!_isWallSliding) {
                        playerRigidbody.linearVelocityX += acceleration * _move.x;
                        animator.SetFloat(WalkSpeedMultiplier, walkingSpeedCurveAnimator.Evaluate(math.abs(_move.x)));
                        playerRigidbody.linearVelocityX =
                            Mathf.Clamp(playerRigidbody.linearVelocityX, -maxSpeed, maxSpeed);
                    }
                }
            }
            else {
                playerRigidbody.linearVelocity = Vector2.zero;
                playerRigidbody.gravityScale = 0;
                playerRigidbody.position += _move * climbSpeed;
                animator.SetBool(Ladder, true);
            }

            if (_isWallSliding) {
                playerRigidbody.linearVelocityY = Mathf.Clamp(playerRigidbody.linearVelocityY, -wallSlideVelocity,
                    float.PositiveInfinity);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.CompareTag("Ladder")) {
                isClimbing = false;
            }
        }

        #endregion

        #region ---Checks---

        private bool GroundCheck() {
            return Physics2D.BoxCast(transform.position.WithOffset(y: 1), new Vector2(0.2f, 1),
                0, Vector2.down, distanceToGround, whatIsGround);
        }

        private bool SolidGroundCheck() {
            // Send a raycast to check if more than half the player is on solid ground
            return Physics2D.Raycast(transform.position.WithOffset(y: 1), Vector2.down, distanceToGround, whatIsGround);
        }

        private bool IsSpawnPositionValid(Vector2 position) {
            // Send a raycast that hits the spike and ground layer. If it hits the ground, return true, else return false.
            RaycastHit2D hit = Physics2D.Raycast(position.WithOffset(y: 1), Vector2.down, distanceToGround,
                LayerMask.GetMask("Ground", "Spike"));
            return hit.collider != null && hit.collider.CompareTag("Ground");
        }

        private Platform PlatformCheck() {
            var hit = Physics2D.BoxCast(transform.position, new Vector2(1, 0.01f),
                0, Vector2.down, distanceToGround, whatIsLadder);

            if (hit.collider == null) {
                debugLogger.Log("Platform Check Failed");
                return null;
            }

            debugLogger.Log("Platform Check Successful");

            return hit.transform.TryGetComponent<Platform>(out var platform) ? platform : null;
        }

        private bool WallCheck() {
            return Physics2D.BoxCast(transform.position.WithOffset(y: 1), new Vector2(1, 2.4f),
                0, Vector2.right * _facingDirection, distanceToWall, whatIsGround);
        }

        private bool LadderCheck() {
            return Physics2D.OverlapBox(transform.position.WithOffset(y: 1), new Vector2(1, 2.4f),
                0, whatIsLadder);
        }

        private void CheckFacingDirection(Vector2 moveDirection) {
            if (moveDirection.x > 0.1f) {
                var transformLocalScale = transform.localScale;
                transform.localScale = transformLocalScale.With(x: 1);
                _facingDirection = 1;
            }
            else if (moveDirection.x < -0.1f) {
                var transformLocalScale = transform.localScale;
                transform.localScale = transformLocalScale.With(x: -1);
                _facingDirection = -1;
            }
        }

        #endregion

        private void Jump() {
            if (_isJumping) return;

            debugLogger.Log("Jumped Pressed");
            playerRigidbody.linearVelocityY = jumpForce;
            canJump = false;
            _lastTimeJumpPressed = Time.time;
            _isJumping = true;
            isGrounded = false;
        }
    }
}