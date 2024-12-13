using Attributes;
using Consensus;
using ExtensionMethods;
using Gaskellgames;
using PlayerLoopTimer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
    public class GravityObjectPlayerMovement : GravityObject {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector]
        private PlayerScript player;

        [GetOrAddComponent, SerializeField, HideInInspector]
        private BoxCollider2D boxCollider;

        [Header("Player Movement")] [SerializeField, ReadOnly]
        public Vector2 inputVector;

        [ReadOnly] public bool isDucking;
        [ReadOnly] public bool isHoldingDuck;
        [ReadOnly] public bool isFacingRight = true;
        [ReadOnly] public bool isClimbing;
        public Vector2 affectingVelocity = Vector2.zero;
        [SerializeField] private float smoothTime;
        [SerializeField] private float maxSpeed;
        [SerializeField] public LayerMask whatIsLadder;
        [SerializeField] private LayerMask whatIsInvalidSpawnPoint;
        [SerializeField] public float climbSpeed;

        [Header("Dash")] [ReadOnly] public bool isDashing;
        [ReadOnly] public bool canDash;
        [SerializeField] private float dashTime;
        [SerializeField] private float dashForce;

        [Header("Jump / Fall")] [ReadOnly] public bool isJumping;
        [ReadOnly] public bool isHoldingJump;
        [ReadOnly] public bool doubleJumping;
        [SerializeField] private float jumpForce;
        [SerializeField] private float minJumpTime;
        [SerializeField] private float coyoteTime;
        [SerializeField] private float jumpBufferTime;
        [SerializeField] public float gravityMultiplier;
        [SerializeField] private float fallSpeed;
        [SerializeField] private LayerMask whatIsGround;
        public bool isActuallyGrounded;

        [Header("Knockback")] [ReadOnly] public bool isTakingKnockback;
        [SerializeField] private float knockBackTime;
        [SerializeField] private float spikeStunTime;
        public int killCharges;
        private Vector2 _knockbackVector;

        [Header("Player Can Move")] [SerializeField, Required]
        private AllInFavourConsensusSO playerCanMove;

        private bool PlayerMovementEnabled => playerCanMove.consensus;

        [Header("Debug Logger")] [SerializeField, Required]
        private DebugLoggerSO debugLogger;

        #region ***---PrivateFields---***
        public CountdownTimer _dashTimer;
        private CountdownTimer _minJumpTimer;
        private CountdownTimer _jumpBufferTimer;
        private CountdownTimer _knockbackTimer;
        private CountdownTimer _coyoteTimer;
        private CountdownTimer _spikeStunTimer;
        private CountdownTimer _platformTimer;

        private Vector2 _smoothMovement;
        private Vector2 _referenceSmoothVelocity;
        [HideInInspector] public Vector2 lastPlatformCheckpoint;
        [HideInInspector] public bool wasGroundedLastFrame;
        
        private IPlayerLadderLogic _playerLadderLogic; 
        [HideInInspector] public LadderMiddle ladderMiddle;
        [HideInInspector] public LadderTop ladderTop;
        #endregion

        #region ***---AnimatorStringToHash---***
        private static readonly int AnimatorDashBool = Animator.StringToHash("IsDashing");
        private static readonly int AnimatorCrouchBool = Animator.StringToHash("IsCrouching");

        private static readonly int AnimatorMovementFloat = Animator.StringToHash("XMovement");
        private static readonly int AnimatorYMovementFloat = Animator.StringToHash("YMovement");

        private static readonly int AnimatorJumpBool = Animator.StringToHash("IsJumping");
        private static readonly int AnimatorIsHoldingJumpBool = Animator.StringToHash("IsHoldingJump");
        private static readonly int AnimatorIsFallingBool = Animator.StringToHash("IsFalling");
        private static readonly int AnimatorClimbBool = Animator.StringToHash("IsClimbing");
        private static readonly int AnimatorGunJumpTrigger = Animator.StringToHash("GunJump");
        #endregion
        #endregion

        protected override void Start() {
            base.Start();

            GravityModifier = gravityMultiplier;

            lastPlatformCheckpoint = transform.position;

            _playerLadderLogic = new PlayerLadderLogic(player);

            InitializeTimers();
        }

        #region ---TimerInitialization---
        private void InitializeTimers() {
            InitializeDashTimer();

            InitializeMinJumpTimer();

            InitializeJumpBufferTimer();

            InitializeKnockbackTimer();

            InitializeSpikeStunTimer();

            InitializeCoyoteTimer();

            InitializePlatformTimer();
        }

        private void InitializeDashTimer() {
            _dashTimer = CountdownTimerBuilder.Start(dashTime)
                .WithOnBegin(() => {
                    player.animator.SetBool(AnimatorDashBool, true);
                    player.healthComponent.TriggerInvulnerability(dashTime - 0.2f);
                })
                .WithOnStop(() => {
                    isDashing = false;
                    lockToAxis = Axis.None;
                    ResetMovement(Axis.YAxis);
                    player.animator.SetBool(AnimatorDashBool, false);
                })
                .Finish();
        }

        private void InitializeMinJumpTimer() {
            _minJumpTimer = CountdownTimerBuilder.Start(minJumpTime)
                .WithOnComplete(() => isJumping = false)
                .Finish();
        }

        private void InitializeJumpBufferTimer() {
            _jumpBufferTimer = CountdownTimerBuilder.Start(jumpBufferTime)
                .WithOnUpdate(() => {
                    if (!isGrounded) return;
                    isJumping = true;
                    ForceExitDuck();
                    _minJumpTimer.StartTimer();
                    _jumpBufferTimer.StopTimer();
                })
                .WithOnComplete(() => isJumping = false)
                .Finish();
        }

        private void InitializeKnockbackTimer() {
            _knockbackTimer = CountdownTimerBuilder.Start(knockBackTime)
                .WithOnComplete(() => isTakingKnockback = false)
                .Finish();
        }

        private void InitializeSpikeStunTimer() {
            _spikeStunTimer = CountdownTimerBuilder.Start(spikeStunTime)
                .WithOnBegin(SpikeStunStart)
                .WithOnStop(SpikeStunStop)
                .Finish();
        }

        private void InitializeCoyoteTimer() {
            _coyoteTimer = CountdownTimerBuilder.Start(coyoteTime)
                .WithOnBegin(() => isGrounded = false)
                .Finish();
        }

        private void InitializePlatformTimer() {
            _platformTimer = CountdownTimerBuilder.Start(minJumpTime)
                .Finish();
        }
        #endregion

        protected override void ComputeVelocity() {
            if (!PlayerMovementEnabled) return;
            
            
            GroundCheck();
            isActuallyGrounded = isGrounded;
            
            if (_playerLadderLogic.LadderLogic()) return;

            player.animator.SetBool(AnimatorIsHoldingJumpBool, isHoldingJump);

            CoyoteTimeLogic();

            var move = Vector2.zero;

            if (isTakingKnockback) {
                velocity = _knockbackVector;
                targetVelocity = _knockbackVector; //changed here
                _smoothMovement = Vector2.SmoothDamp(_smoothMovement, _knockbackVector.normalized,
                    ref _referenceSmoothVelocity, smoothTime);

                return;
            }

            move.x = inputVector.x;

            player.animator.SetBool(AnimatorIsFallingBool, false);

            // if (isGrounded && isJumping) {
            //     velocity.y = jumpForce;
            // } else
            if (doubleJumping && isJumping) {
                MovingPlatformExit();
                ResetMovement(Axis.YAxis);
                isDashing = false;
                velocity.y = jumpForce;
                // targetVelocity.y = jumpForce;
                doubleJumping = false;
            }
            else if (!isHoldingJump && !isJumping) {
                if (velocity.y > 0) velocity.y *= fallSpeed;
                targetVelocity.y *= fallSpeed;
                player.animator.SetBool(AnimatorIsFallingBool, true);
            }
            else if (isJumping) {
                MovingPlatformExit();
                velocity.y = jumpForce;
                // targetVelocity.y = jumpForce;
            }

            if (isDashing) {
                targetVelocity.x = dashForce;
                _smoothMovement = Vector2.SmoothDamp(_smoothMovement, new Vector2(dashForce, 0).normalized,
                    ref _referenceSmoothVelocity, smoothTime);
            }
            else {
                if (isHoldingDuck && !isJumping && !isDucking && isActuallyGrounded) {
                    EnterDuck();
                }
                else if (isDucking && !isHoldingDuck) {
                    ForceExitDuck();
                }

                if (isDucking) {
                    isFacingRight = inputVector.x switch {
                        > 0.01f => true,
                        < -0.01f => false,
                        _ => isFacingRight
                    };
                    move = Vector2.zero;
                }

                _smoothMovement = Vector2.SmoothDamp(_smoothMovement, move, ref _referenceSmoothVelocity, smoothTime);

                player.animator.SetFloat(AnimatorMovementFloat, Mathf.Abs(_smoothMovement.x));

                targetVelocity.x = _smoothMovement.x * maxSpeed;

                targetVelocity += affectingVelocity;
            }

            if (!player.isAttacking) {
                var transformLocalScale = transform.localScale;
                switch (_smoothMovement.x) {
                    case > 0.01f:
                        isFacingRight = true;
                        transform.localScale = transformLocalScale.With(1);
                        break;
                    case < -0.01f:
                        isFacingRight = false;
                        transform.localScale = transformLocalScale.With(-1);
                        break;
                }
            }

            if (Mathf.Abs(velocity.x) < 0.01f) {
                _smoothMovement.x = 0;
            }

            if (isGrounded) {
                if (!isDashing) canDash = true;
                killCharges = 0;

                if (!isJumping) {
                    player.animator.SetBool(AnimatorJumpBool, false);
                }
            }

            if (_smoothMovement.x > 0.1f)
                isFacingRight = true;
            else if (_smoothMovement.x < -0.1f)
                isFacingRight = false;

            wasGroundedLastFrame = isActuallyGrounded;
        }

        /*private bool IsValidSpawnPoint() {
            var quantizedPosition =
                new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y) - .3f);
            var hitColliders = Physics.OverlapBox(quantizedPosition, transform.localScale,
                Quaternion.identity, whatIsInvalidSpawnPoint);
            var i = 0;
            while (i < hitColliders.Length) {
                if (hitColliders[i].CompareTag("Hazard")) {
                    return false;
                }

                i++;
            }

            var hit = Physics2D.Raycast(quantizedPosition, Vector2.down, .5f);
            return hit.collider != null && !hit.collider.CompareTag("Hazard") && hit.collider.CompareTag("Ground");
        }*/

        #region ---EventFunctions---
        public void OnJump(InputValue inputValue) {
            isHoldingJump = inputValue.isPressed;
            if (!isHoldingJump) return;

            ForceExitDuck();
            _playerLadderLogic.ExitLadder();
            if (isGrounded || killCharges > 0) {
                isJumping = true;
                _minJumpTimer.StartTimer();
                _coyoteTimer.StopTimer();
                player.animator.SetBool(AnimatorJumpBool, true);

                if (killCharges > 0) {
                    doubleJumping = true;
                    killCharges--;
                    player.animator.SetTrigger(AnimatorGunJumpTrigger);
                }
            }
            else {
                _jumpBufferTimer.StartTimer();
            }
        }

        public void OnMove(InputValue inputValue) {
            inputVector = inputValue.Get<Vector2>();
        }

        public void OnDash(InputValue inputValue) {
            if (!inputValue.isPressed) return;
            if (player.isAttacking) return;
            if (isDashing || !canDash) {
                _coyoteTimer.StopTimer();

                if (killCharges > 0) {
                    killCharges--;
                }
                else {
                    return;
                }
            }

            _playerLadderLogic.ExitLadder();
            ForceExitDuck();
            isDashing = true;
            canDash = false;
            lockToAxis = Axis.XAxis;
            _dashTimer.StartTimer();

            if (inputVector.x > 0.1f)
                dashForce = Mathf.Abs(dashForce);
            else if (inputVector.x < -0.1f)
                dashForce = -Mathf.Abs(dashForce);
            else {
                if (isFacingRight)
                    dashForce = Mathf.Abs(dashForce);
                else
                    dashForce = -Mathf.Abs(dashForce);
            }
        }

        public void OnDuck(InputValue inputValue) {
            isHoldingDuck = inputValue.isPressed;
        }
        #endregion

        public void ForceExitDuck() {
            debugLogger.Log("exiting duck");
            isDucking = false;
            boxCollider.size = new Vector2(0.9f, 1.9f);
            boxCollider.offset = new Vector2(0f, 1f);
            player.animator.SetBool(AnimatorCrouchBool, false);
        }

        private void EnterDuck() {
            debugLogger.Log("entering duck");

            if (_playerLadderLogic.TryGetLadderTop(out ladderTop)) return;
            
            isDucking = true;
            player.animator.SetBool(AnimatorCrouchBool, true);
            boxCollider.size = new Vector2(0.9f, 0.9f);
            boxCollider.offset = new Vector2(0f, 0.5f);
            debugLogger.Log("Changed player scale down");
        }

        public void Knockback(Vector2 argumentKnockback) {
            isTakingKnockback = true;

            _knockbackTimer.StartTimer();

            _knockbackVector = argumentKnockback;
        }

        private void SpikeStunStart() {
            //_player.spriteRenderer.color = new Color(1, 1, 1, 0.1f);
            player.healthComponent.TriggerInvulnerability(spikeStunTime);
            ResetMovement(0);
            _knockbackVector = Vector2.zero;
            isTakingKnockback = true;
        }

        private void SpikeStunStop() {
            //_player.spriteRenderer.color = new Color(1, 1, 1, 1);
            gameObject.transform.position = lastPlatformCheckpoint;
            isTakingKnockback = false;
        }

        public void SpikeReset() {
            //gameObject.transform.position = lastPlatformCheckpoint;
            ResetMovement(0);
            _spikeStunTimer.StartTimer();
            isDashing = false;
        }

        public void MovingPlatformEnter() {
            if (_platformTimer.IsRunning) return;
            GravityModifier = 0f;
        }

        public void MovingPlatformExit() {
            GravityModifier = gravityMultiplier;
        }


#if UNITY_EDITOR
        private void OnValidate() {
            if (_jumpBufferTimer != null) _jumpBufferTimer.Duration = jumpBufferTime;
            if (_dashTimer != null) _dashTimer.Duration = dashTime;
            if (_minJumpTimer != null) _minJumpTimer.Duration = minJumpTime;
            if (_knockbackTimer != null) _knockbackTimer.Duration = knockBackTime;
            if (_coyoteTimer != null) _coyoteTimer.Duration = coyoteTime;
            if (_spikeStunTimer != null) _spikeStunTimer.Duration = spikeStunTime;
        }
#endif

        private void CoyoteTimeLogic() {
            if (!isActuallyGrounded && wasGroundedLastFrame && !isJumping && !isDashing) {
                debugLogger.Log("Start coyote timer");
                _coyoteTimer.RestartTimer();
            }

            if (_coyoteTimer.IsRunning) {
                debugLogger.Log("Timer running");
                isGrounded = true;
            }
        }

        private void GroundCheck() {
            isGrounded = Physics2D.OverlapBox(transform.position, new Vector2(0.8f, 0.01f), 0, whatIsGround);
        }
    }
}