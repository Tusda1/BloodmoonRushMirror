using System;
using Attributes;
using Consensus;
using ExtensionMethods;
using Gaskellgames;
using PlayerLoopTimer;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
    public class GravityObjectPlayerMovement : GravityObject {
        #region ---Fields---
        [Header("References")]
        [GetOrAddComponent, SerializeField] private PlayerScript _player;
        [GetOrAddComponent, SerializeField] private BoxCollider2D _boxCollider;

        [Header("Player Movement")] 
        [SerializeField, ReadOnly] private Vector2 inputVector;
        
        [ReadOnly] public bool isDucking;
        [ReadOnly] public bool isHoldingDuck;
        [ReadOnly] public bool isFacingRight = true;
        [ReadOnly] public bool isClimbing;
        public Vector2 affectingVelocity = Vector2.zero;
        [SerializeField] private float smoothTime;
        [SerializeField] private float maxSpeed;
        [SerializeField] private LayerMask whatIsLadder;
        [SerializeField] private LayerMask whatIsInvalidSpawnPoint;
        [SerializeField] private float climbSpeed;

        [Header("Dash")] 
        [ReadOnly] public bool isDashing;
        [ReadOnly] public bool canDash;
        [SerializeField] private float dashTime;
        [SerializeField] private float dashForce;

        [Header("Jump / Fall")] 
        [ReadOnly] public bool isJumping;
        [ReadOnly] public bool isHoldingJump;
        [ReadOnly] public bool doubleJumping;
        [SerializeField] private float jumpForce;
        [SerializeField] private float minJumpTime;
        [SerializeField] private float coyoteTime;
        [SerializeField] private float jumpBufferTime;
        [SerializeField] private float gravityMultiplier;
        [SerializeField] private float fallSpeed;
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private bool isActuallyGrounded;

        [Header("Knockback")]
        [ReadOnly] public bool isTakingKnockback;
        [SerializeField] private float knockBackTime;
        [SerializeField] private float spikeStunTime;
        public int killCharges;
        private Vector2 _knockbackVector;

        [Header("Player Can Move")] 
        [SerializeField, Required] private AllInFavourConsensusSO playerCanMove;
        private bool PlayerMovementEnabled => playerCanMove.consensus;

        [Header("Debug Logger")] [SerializeField, Required]
        private DebugLoggerSO debugLogger;

        #region ***---PrivateFields---***
        private CountdownTimer _dashTimer;
        private CountdownTimer _minJumpTimer;
        private CountdownTimer _jumpBufferTimer;
        private CountdownTimer _knockbackTimer;
        private CountdownTimer _coyoteTimer;
        private CountdownTimer _spikeStunTimer;
        
        private Vector2 _smoothMovement;
        private Vector2 _referenceSmoothVelocity;
        private Vector2 _lastSpikePoint;
        [HideInInspector] public bool _wasGroundedLastFrame;
        private Ladder _ladder;
        private bool _notClimbing;
        #endregion

        #region ***---AnimatorStringToHash---***
        private static readonly int AnimatorDashTrigger = Animator.StringToHash("DashStart");
        private static readonly int AnimatorDashBool = Animator.StringToHash("IsDashing");
        private static readonly int AnimatorDuckTrigger = Animator.StringToHash("DuckStart");
        private static readonly int AnimatorDuckBool = Animator.StringToHash("IsDucking");
        #endregion
        #endregion

        protected override void Start() {
            base.Start();
            
            GravityModifier = gravityMultiplier;

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
        }

        private void InitializeDashTimer() {
            _dashTimer = new CountdownTimer(dashTime);
            _dashTimer.OnBegin += () => {
                _player.animator.SetTrigger(AnimatorDashTrigger);
                _player.animator.SetBool(AnimatorDashBool, true);
                _player.healthComponent.TriggerInvulnerability(dashTime);
            };
            _dashTimer.OnStop += () => {
                lockToAxis = 0;
                isDashing = false;
                ResetMovement(2);
                _player.animator.SetBool(AnimatorDashBool, false);
            };
        }

        private void InitializeMinJumpTimer() {
            _minJumpTimer = new CountdownTimer(minJumpTime);
            _minJumpTimer.OnComplete += () => isJumping = false;
        }

        private void InitializeJumpBufferTimer() {
            _jumpBufferTimer = new CountdownTimer(jumpBufferTime);
            _jumpBufferTimer.OnUpdate += () => {
                if (!isGrounded) return;
                isJumping = true;
                ForceExitDuck();
                _minJumpTimer.StartTimer();
                _jumpBufferTimer.StopTimer();
            };
            _jumpBufferTimer.OnComplete += () => isJumping = false;
        }

        private void InitializeKnockbackTimer() {
            _knockbackTimer = new CountdownTimer(knockBackTime);
            _knockbackTimer.OnComplete += () => isTakingKnockback = false;
        }

        private void InitializeSpikeStunTimer() {
            _spikeStunTimer = new CountdownTimer(spikeStunTime);
            _spikeStunTimer.OnBegin += SpikeStunStart;
            _spikeStunTimer.OnComplete += SpikeStunStop;
        }

        private void InitializeCoyoteTimer() {
            _coyoteTimer = new CountdownTimer(coyoteTime);
            _coyoteTimer.OnBegin += () => isGrounded = false;
        }
        #endregion

        protected override void ComputeVelocity() {
            if (!PlayerMovementEnabled) return;

            if (LadderLogic()) return;

            GroundCheck();
            isActuallyGrounded = isGrounded;

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

            // if (isGrounded && isJumping) {
            //     velocity.y = jumpForce;
            // } else
            if (doubleJumping && isJumping) {
                ResetMovement(2);
                isDashing = false;
                velocity.y = jumpForce;
                targetVelocity.y = jumpForce;
                doubleJumping = false;
            }
            else if (!isHoldingJump && !isJumping) {
                if (velocity.y > 0)
                    velocity.y *= fallSpeed;
                targetVelocity.y *= fallSpeed;
            }
            else if (isJumping) {
                velocity.y = jumpForce;
                targetVelocity.y = jumpForce;
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

                targetVelocity.x = (_smoothMovement.x * maxSpeed);
                
                targetVelocity += affectingVelocity;
                
            }

            if (!_player.isAttacking) {
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
            }

            if (isActuallyGrounded && IsValidSpawnPoint())
                _lastSpikePoint = new Vector2(Mathf.Round(transform.position.x),
                    Mathf.Round(transform.position.y) - 0.4f);

            _wasGroundedLastFrame = isActuallyGrounded;
        }

        private bool IsValidSpawnPoint() {
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
        }

        #region ---EventFunctions---
        public void OnJump(InputValue inputValue) {
            isHoldingJump = inputValue.isPressed;
            if (!isHoldingJump) return;
            
            ForceExitDuck();
            Debug.Log(string.Join('|', isGrounded, isActuallyGrounded, _coyoteTimer.IsRunning));
            if (isGrounded || killCharges > 0) {
                isJumping = true;
                _minJumpTimer.StartTimer();
                _coyoteTimer.StopTimer();

                if (killCharges > 0) {
                    doubleJumping = true;
                    killCharges--;
                }
            }
            else {
                _jumpBufferTimer.StartTimer();
            }
        }

        public void OnMove(InputValue inputValue) {
            var referenceVector = inputValue.Get<Vector2>();
            inputVector = referenceVector.normalized;
        }

        public void OnDash(InputValue inputValue) {
            if (!inputValue.isPressed) return;
            if (isDashing || !canDash) {
                _coyoteTimer.StopTimer();

                if (killCharges > 0) {
                    killCharges--;
                }
                else {
                    return;
                }
            }

            ForceExitDuck();
            isDashing = true;
            canDash = false;
            lockToAxis = 1;
            _dashTimer.StartTimer();

            if (isFacingRight)
                dashForce = Mathf.Abs(dashForce);
            else
                dashForce = -Mathf.Abs(dashForce);
        }

        public void OnDuck(InputValue inputValue) {
            isHoldingDuck = inputValue.isPressed;
        }

        private void OnTriggerStay2D(Collider2D other) {
            if (!other.CompareTag("Ladder")) return;

            _notClimbing = false;
            _ladder = other.GetComponentInChildren<Ladder>();
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag("LadderShell")) return;

            _notClimbing = true;
            if (_ladder != null) {
                _ladder.LadderIsUsed = false;
                _ladder = null;
            }
        }
        #endregion

        private void ForceExitDuck() {
            debugLogger.Log("exiting duck");
            isDucking = false;
            _player.animator.SetBool(AnimatorDuckBool, false);
        }

        private void EnterDuck() {
            debugLogger.Log("entering duck");

            if (TryGetLadder(out _ladder)) {
                transform.position = _ladder.ladderStartPosition.position;
                isClimbing = true;
                isGrounded = false;
                _notClimbing = false;
                _ladder.LadderIsUsed = true;
                debugLogger.Log("Enter ladder from top");

                return;
            }

            isDucking = true;
            _player.animator.SetTrigger(AnimatorDuckTrigger);
            _player.animator.SetBool(AnimatorDuckBool, true);
            debugLogger.Log("Changed player scale down");
        }

        public void Knockback(Vector2 argumentKnockback) {
            isTakingKnockback = true;

            _knockbackTimer.StartTimer();

            _knockbackVector = argumentKnockback;
        }

        private void SpikeStunStart() {
            _player.spriteRenderer.color = new Color(1, 1, 1, 0.1f);
            _player.healthComponent.TriggerInvulnerability(0.75f);
            ResetMovement(0);
            _knockbackVector = Vector2.zero;
            isTakingKnockback = true;
        }

        private void SpikeStunStop() {
            _player.spriteRenderer.color = new Color(1, 1, 1, 1);
            isTakingKnockback = false;
        }

        public void SpikeReset() {
            gameObject.transform.position = _lastSpikePoint;
            ResetMovement(0);
            _spikeStunTimer.StartTimer();
            isDashing = false;
        }

        public void MovingPlatformEnter() {
            GravityModifier = 0f;
        }

        public void MovingPLatformExit() {
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

        private bool LadderLogic() {
            if (inputVector.y > 0 && _ladder != null && !isClimbing) {
                isClimbing = true;
                _ladder.LadderIsUsed = true;
                isGrounded = false;
                _notClimbing = false;
                debugLogger.Log("Enter ladder from bottom");
            }

            if (isClimbing) {
                targetVelocity = new Vector2(0, inputVector.y * climbSpeed);
                if (_ladder != null) {
                    var transformPosition = transform.position;
                    transform.position = transformPosition.With(x: _ladder.triggerCollider.transform.position.x);
                }

                GravityModifier = 0;

                if ((isGrounded && inputVector.y < 0) || isHoldingJump || isDashing || _notClimbing) {
                    debugLogger.Log(
                        $"Exit Ladder | {isGrounded && inputVector.y < 0} || {isHoldingJump} || {isDashing} || {_notClimbing}");

                    isClimbing = false;
                    // isGrounded = true;
                    GravityModifier = gravityMultiplier;
                    if (_ladder != null) {
                        _ladder.LadderIsUsed = false;
                        _ladder = null;
                    }

                    _notClimbing = false;
                }
                else {
                    isGrounded = false;
                    ForceExitDuck();
                    return true;
                }
            }

            return false;
        }

        private void CoyoteTimeLogic() {
            if (!isActuallyGrounded && _wasGroundedLastFrame && !isJumping && !isDashing) {
                Debug.Log("Start coyote timer");
                _coyoteTimer.RestartTimer();
            }

            if (_coyoteTimer.IsRunning) {
                Debug.Log("Timer running");
                isGrounded = true;
            }
        }

        private bool TryGetLadder(out Ladder platform) {
            debugLogger.Log("TryGetLadder");
            var ladder = Physics2D.OverlapBox(transform.position.WithOffset(transform.localScale.y * 0.5f),
                transform.localScale.Abs(), 0, whatIsLadder);

            if (ladder == null || ladder.CompareTag("Ladder")) {
                platform = null;
                return false;
            }

            platform = ladder.GetComponentInChildren<Ladder>();

            if (ladder != null && platform == null) {
                throw new Exception(
                    $"{nameof(Ladder)} was detected but with no {nameof(Ladder)} component were found on it!");
            }

            if (ladder != null && platform != null) {
                debugLogger.Log("Ladder found");
            }

            return ladder;
        }

        private void GroundCheck() {
            isGrounded = Physics2D.OverlapBox(transform.position, new Vector2(0.5f, 0.01f), 0, whatIsGround);
        }
    }
}