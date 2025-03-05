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

        [Header("Player Movement")] 
        [SerializeField, ReadOnly] public Vector2 inputVector;

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

        [Header("Dash")] 
        [ReadOnly] public bool isDashing;
        [ReadOnly] public bool canDash;
        [SerializeField] private float dashTime;
        [SerializeField] private float dashCooldown;
        [SerializeField] private float dashForce;

        [Header("Jump / Fall")] 
        [ReadOnly] public bool isJumping;
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
        
        [Header("Particles")] 
        [Attributes.Required] public ParticleSystem lameParticles;
        [Attributes.Required] public ParticleSystem coolParticles;
        
        [Header("Player Can Move")] [SerializeField, Attributes.Required]
        private AllInFavourConsensusSO playerCanMove;

        private bool PlayerMovementEnabled => playerCanMove.consensus;

        [Header("Debug Loggers")] 
        [SerializeField, Attributes.Required] private DebugLoggerSO playerMovementDebugLogger;
        [SerializeField, Attributes.Required] private DebugLoggerSO ladderDebugLogger;

        #region ***---PrivateFields---***
        public CountdownTimer DashTimer;
        public CountdownTimer DashCooldownTimer;
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
        
        public IPlayerLadderLogic PlayerLadderLogic;
        
        private static readonly int AnimatorRespawnTrigger = Animator.StringToHash("Respawn");
        #endregion

        #region ***---AnimatorStringToHash---***
        private static readonly int AnimatorDashBool = Animator.StringToHash("IsDashing");
        private static readonly int AnimatorCrouchBool = Animator.StringToHash("IsCrouching");

        private static readonly int AnimatorMovementFloat = Animator.StringToHash("XMovement");

        private static readonly int AnimatorJumpBool = Animator.StringToHash("IsJumping");
        private static readonly int AnimatorIsHoldingJumpBool = Animator.StringToHash("IsHoldingJump");
        private static readonly int AnimatorIsFallingBool = Animator.StringToHash("IsFalling");
        private static readonly int AnimatorGunJumpTrigger = Animator.StringToHash("GunJump");
        #endregion
        #endregion

        protected override void Start() {
            base.Start();

            GravityModifier = gravityMultiplier;

            lastPlatformCheckpoint = transform.position;

            PlayerLadderLogic = new PlayerLadderLogic(player, ladderDebugLogger);

            InitializeTimers();
        }

        #region ---TimerInitialization---
        private void InitializeTimers() {
            InitializeDashTimer();
            
            InitializeDashCooldownTimer();

            InitializeMinJumpTimer();

            InitializeJumpBufferTimer();

            InitializeKnockbackTimer();

            InitializeSpikeStunTimer();

            InitializeCoyoteTimer();

            InitializePlatformTimer();
        }

        private void InitializeDashTimer() {
            DashTimer = CountdownTimerBuilder.Start(dashTime)
                .WithOnBegin(() => {
                    player.animator.SetBool(AnimatorDashBool, true);
                    player.healthComponent.HasInvincibilityFrames = true;
                })
                .WithOnStop(() => {
                    isDashing = false;
                    lockToAxis = Axis.None;
                    ResetMovement(Axis.YAxis);
                    player.healthComponent.HasInvincibilityFrames = false;
                    player.animator.SetBool(AnimatorDashBool, false);
                })
                .Finish();
        }

        private void InitializeDashCooldownTimer() {
            DashCooldownTimer = CountdownTimerBuilder.Start(dashCooldown).Finish();
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
                .WithOnComplete(() => {
                    if (!_spikeStunTimer.IsRunning) {
                        isTakingKnockback = false;
                    }
                    else {
                        if (player.healthComponent.TotalHealth > 0) gameObject.transform.position = lastPlatformCheckpoint;
                        player.animator.SetTrigger(AnimatorRespawnTrigger);
                    }
                })
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
            if (isTakingKnockback) {
                velocity = _knockbackVector;
                targetVelocity = _knockbackVector; //changed here
                _smoothMovement = Vector2.SmoothDamp(_smoothMovement, _knockbackVector.normalized,
                    ref _referenceSmoothVelocity, smoothTime);

                return;
            }
            
            GroundCheck();
            isActuallyGrounded = isGrounded;
            
            if (PlayerLadderLogic.LadderLogic()) return;
            

            player.animator.SetBool(AnimatorIsHoldingJumpBool, isHoldingJump);
            player.animator.SetBool(AnimatorIsFallingBool, isFalling);

            if (!PlayerMovementEnabled) {
                player.animator.SetBool(AnimatorJumpBool, false);
                return;
            }

            CoyoteTimeLogic();

            var move = Vector2.zero;

            move.x = inputVector.x;

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
                // player.animator.SetBool(AnimatorIsFallingBool, true);
            }
            else if (isJumping) {
                MovingPlatformExit();
                velocity.y = jumpForce;
                player.animator.SetBool(AnimatorJumpBool, true);
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

            if (!wasGroundedLastFrame && isActuallyGrounded) {
                player.AudioManager.PlayOneShot(PlayerScript.PlayerStep_SoundID);
            }

            wasGroundedLastFrame = isActuallyGrounded;
            
        }

        #region ---EventFunctions---
        public void OnJump(InputValue inputValue) {
            isHoldingJump = inputValue.isPressed;
            if (!isHoldingJump) return;
            
            ForceExitDuck();
            PlayerLadderLogic.ExitLadder();
             if (isGrounded || killCharges > 0) {
                isDashing = false;
                lockToAxis = Axis.None;
                ResetMovement(Axis.None);
                isJumping = true;
                _minJumpTimer.StartTimer();
                _coyoteTimer.StopTimer();
                player.animator.SetBool(AnimatorJumpBool, true);

                if (killCharges > 0) {
                    doubleJumping = true;
                    killCharges--;
                    player.animator.SetTrigger(AnimatorGunJumpTrigger);
                    
                    player.AudioManager.PlayOneShot(PlayerScript.PlayerGunJump_SoundID);
                    
                    coolParticles.Play();
                }
            }
            else {
                _jumpBufferTimer.StartTimer();
            }
        }

        public void OnMove(InputValue inputValue) {
            inputVector = inputValue.Get<Vector2>();

            inputVector.x = inputVector.x switch {
                > 0.7f or < -0.7f => Mathf.Sign(inputVector.x),
                < -0.3f or > 0.3f => Mathf.Sign(inputVector.x) * 0.4f,
                _ => 0
            };

            inputVector.y = inputVector.y switch {
                > 0.8f or < -0.8f => Mathf.Sign(inputVector.y),
                < -0.6f or > 0.6f => Mathf.Sign(inputVector.y) * 0.4f,
                _ => 0
            };
        }

        public void OnDash(InputValue inputValue) {
            if (!inputValue.isPressed) return;
            if (player.isAttacking) return;
            if (DashCooldownTimer.IsRunning) {
                //Play sfx here indicating dash timer not out maybe?
                return;
            }
            if (isDashing || !canDash) {
                _coyoteTimer.StopTimer();

                if (killCharges > 0) {
                    killCharges--;
                }
                else {
                    return;
                }
            }

            PlayerLadderLogic.ExitLadder();
            ForceExitDuck();
            isDashing = true;
            DashCooldownTimer.StartTimer();
            player.AudioManager.PlayOneShot(PlayerScript.PlayerDash_SoundID);
            canDash = false;
            lockToAxis = Axis.XAxis;
            DashTimer.StartTimer();

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
            playerMovementDebugLogger.Log("exiting duck");
            isDucking = false;
            boxCollider.size = new Vector2(0.9f, 1.9f);
            boxCollider.offset = new Vector2(0f, 1f);
            player.animator.SetBool(AnimatorCrouchBool, false);
        }

        private void EnterDuck() {
            playerMovementDebugLogger.Log("entering duck");

            if (PlayerLadderLogic.TryGetLadderTop(out _)) return;
            
            isDucking = true;
            player.animator.SetBool(AnimatorCrouchBool, true);
            boxCollider.size = new Vector2(0.9f, 0.9f);
            boxCollider.offset = new Vector2(0f, 0.5f);
            playerMovementDebugLogger.Log("Changed player scale down");
        }

        public void Knockback(Vector2 argumentKnockback) {
            isTakingKnockback = true;

            _knockbackVector = argumentKnockback;
            
            _knockbackTimer.StartTimer();
        }

        private void SpikeStunStart() {
            //_player.spriteRenderer.color = new Color(1, 1, 1, 0.1f);
            player.healthComponent.TriggerInvulnerability(spikeStunTime);
            ResetMovement(0);
            _knockbackVector = Vector2.zero;
            isTakingKnockback = true;
            print("Starting spike stun");
        }

        private void SpikeStunStop() {
            //_player.spriteRenderer.color = new Color(1, 1, 1, 1);
            //if (player.healthComponent.TotalHealth > 0) gameObject.transform.position = lastPlatformCheckpoint;
            isTakingKnockback = false;
        }

        public void SpikeReset() {
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
            if (DashTimer != null) DashTimer.Duration = dashTime;
            if (_minJumpTimer != null) _minJumpTimer.Duration = minJumpTime;
            if (_knockbackTimer != null) _knockbackTimer.Duration = knockBackTime;
            if (_coyoteTimer != null) _coyoteTimer.Duration = coyoteTime;
            if (_spikeStunTimer != null) _spikeStunTimer.Duration = spikeStunTime;
        }
#endif

        private void CoyoteTimeLogic() {
            if (!isActuallyGrounded && wasGroundedLastFrame && !isJumping && !isDashing) {
                playerMovementDebugLogger.Log("Start coyote timer");
                _coyoteTimer.RestartTimer();
            }

            if (_coyoteTimer.IsRunning) {
                playerMovementDebugLogger.Log("Timer running");
                isGrounded = true;
            }
        }

        private void GroundCheck() {
            isGrounded = Physics2D.OverlapBox(transform.position, new Vector2(0.8f, 0.01f), 0, whatIsGround);
        }
    }
}