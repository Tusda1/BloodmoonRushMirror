using Attributes;
using Consensus;
using PlayerLoopTimer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
    public class GravityObjectPlayerMovement : GravityObject {
        #region ---Fields---
        public bool isDucking;
        public bool isHoldingDuck;
        public bool isFacingRight = true;
        [SerializeField] private float smoothTime;
        [SerializeField] private float maxSpeed;

        [Header("Dash")] public bool isDashing;
        public bool canDash;
        [SerializeField] private float dashTime;
        [SerializeField] private float dashForce;

        [Header("Jump / Fall")] public bool isJumping;
        public bool isHoldingJump;
        public bool doubleJumping;
        [SerializeField] private float jumpForce;
        [SerializeField] private float minJumpTime;
        [SerializeField] private float coyoteTime;
        [SerializeField] private float jumpBufferTime;
        [SerializeField] private float gravityMultiplier;
        [SerializeField] private float fallSpeed;

        [Header("Knockback")] public bool isKnockbacked;
        [SerializeField] private float knockBackTime;
        public Vector2 knockbackVector;
        public int killCharges;

        [Header("Player Can Move")] [SerializeField, Required]
        private AllInFavourConsensus playerCanMove;

        private bool playerMovementEnabled => playerCanMove.consensus;

        [Header("Debug Logger")] [SerializeField, Required]
        private DebugLoggerSO debugLogger;

        #region ***---PrivateFields---***
        [GetOrAddComponent] private BoxCollider2D _boxCollider;
        private CountdownTimer _dashTimer;
        private CountdownTimer _minJumpTimer;
        private CountdownTimer _jumpBufferTimer;
        private CountdownTimer _knockbackTimer;
        private CountdownTimer _coyoteTimer;
        private Vector2 _inputVector;
        private Vector2 _smoothMovement;
        private Vector2 _referenceSmoothVelocity;
        private bool _wasGroundedLastFrame;
        #endregion

        private new void Start() {
            gravityModifier = gravityMultiplier;

            InitializeTimers();
        }
        #endregion

        #region ComputeVelocity
        protected override void ComputeVelocity() {
            if (_wasGroundedLastFrame && !isJumping) {
                _coyoteTimer.StartTimer();
            }

            if (isJumping) {
                _coyoteTimer.StopTimer();
            }

            if (isGrounded == false) {
                isGrounded = _coyoteTimer.TimerRunning;
            }

            var move = Vector2.zero;

            if (!playerMovementEnabled) return;

            if (isKnockbacked) {
                velocity.y = knockbackVector.y;
                targetVelocity.x = knockbackVector.x;
                _smoothMovement = Vector2.SmoothDamp(_smoothMovement, knockbackVector.normalized,
                    ref _referenceSmoothVelocity, smoothTime);

                return;
            }

            move.x = _inputVector.x;

            if (isGrounded && isJumping) {
                velocity.y = jumpForce;
            }
            else if (doubleJumping && isJumping) {
                ResetMovement(2);
                isDashing = false;
                velocity.y = jumpForce;
                doubleJumping = false;
            }
            else if (!isHoldingJump && !isJumping) {
                if (velocity.y > 0)
                    velocity.y *= fallSpeed;
            }

            if (isDashing) {
                targetVelocity.x = dashForce;
                _smoothMovement = Vector2.SmoothDamp(_smoothMovement, new Vector2(dashForce, 0).normalized,
                    ref _referenceSmoothVelocity, smoothTime);
            }
            else {
                if (isGrounded) {
                    canDash = true;
                    killCharges = 0;
                }

                if (isHoldingDuck && !isJumping && !isDucking && isGrounded) {
                    EnterDuck();
                }
                else if (isDucking && !isHoldingDuck) {
                    ForceExitDuck();
                }

                if (isDucking) {
                    if (_inputVector.x > 0.01f) isFacingRight = true;
                    else if (_inputVector.x < -0.01f) isFacingRight = false;
                    move = Vector2.zero;
                }

                _smoothMovement = Vector2.SmoothDamp(_smoothMovement, move, ref _referenceSmoothVelocity, smoothTime);

                targetVelocity = _smoothMovement * maxSpeed;

                if (_smoothMovement.x > 0.01f) isFacingRight = true;
                else if (_smoothMovement.x < -0.01f) isFacingRight = false;
            }

            if (Mathf.Abs(GetVelocity().x) < 0.01f) {
                _smoothMovement.x = 0;
            }

            _wasGroundedLastFrame = isGrounded;
        }
        #endregion

        #region Functions
        public void Jump(InputAction.CallbackContext context) {
            if (context.performed) {
                isHoldingJump = true;
                ForceExitDuck();
                if (isGrounded || killCharges > 0) {
                    isJumping = true;
                    _minJumpTimer.StartTimer();

                    if (killCharges > 0) {
                        doubleJumping = true;
                        killCharges--;
                    }
                }
                else {
                    _jumpBufferTimer.StartTimer();
                }
            }

            if (context.canceled) {
                isHoldingJump = false;
                // if (jumpBufferTimer.TimerRunning) 
                // {
                //     jumpBufferTimer.StopTimer();
                //     isJumping = false;
                // }
            }
        }

        public void Run(InputAction.CallbackContext context) {
            Vector2 referenceVector = context.ReadValue<Vector2>();
            _inputVector.x = referenceVector.normalized.x;
        }

        public void Dash(InputAction.CallbackContext context) {
            if (!context.performed) return;
            if (isDashing || !canDash) {
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

        public void TestFunction(InputAction.CallbackContext context) {
            killCharges += 1;
            Knockback(new Vector2(1, 0), true);
        }

        public void DuckInput(InputAction.CallbackContext context) {
            if (context.performed) {
                isHoldingDuck = true;
            }

            if (context.canceled) {
                isHoldingDuck = false;
            }
        }

        private void ForceExitDuck() {
            print("exiting duck");
            isDucking = false;
            //boxCollider.size = new Vector2(boxCollider.size.x, 2);
            gameObject.transform.localScale = new Vector3(1, 1, 1); //TEST
        }

        private void EnterDuck() {
            print("entering duck");
            isDucking = true;
            //boxCollider.size = new Vector2(boxCollider.size.x, 2);
            gameObject.transform.localScale = new Vector3(1.5f, .5f, 1); //TEST
        }

        public void Knockback(Vector2 enemyPosition, bool givesKnockback) {
            if (!givesKnockback) return;

            isKnockbacked = true;

            _knockbackTimer.StartTimer();

            if (enemyPosition.x < transform.position.x) {
                knockbackVector.x = Mathf.Abs(knockbackVector.x);
            }
            else {
                knockbackVector.x = -Mathf.Abs(knockbackVector.x);
            }
        }
        #endregion

        #region Timers
        private void MinJumpCancel() {
            isJumping = false;
        }

        private void CompleteDash() {
            _dashTimer.StopTimer();
            lockToAxis = 0;
            isDashing = false;
            ResetMovement(2);
        }

        private void JumpBuffer() {
            if (isGrounded) {
                isJumping = true;
                ForceExitDuck();
                _minJumpTimer.StartTimer();
                _jumpBufferTimer.StopTimer();
            }
        }

        private void CompleteJumpBuffer() {
            isJumping = false;
        }

        private void FinishKnockBack() {
            isKnockbacked = false;
        }

        private void CoyoteCheck() {
            if (isGrounded || isJumping) {
                _coyoteTimer.StopTimer();
            }
        }

        private void InitializeTimers() {
            _dashTimer = new CountdownTimer(dashTime);
            _dashTimer.OnComplete += CompleteDash;

            _minJumpTimer = new CountdownTimer(minJumpTime);
            _minJumpTimer.OnComplete += MinJumpCancel;

            _jumpBufferTimer = new CountdownTimer(jumpBufferTime);
            _jumpBufferTimer.OnUpdate += JumpBuffer;
            _jumpBufferTimer.OnComplete += CompleteJumpBuffer;

            _knockbackTimer = new CountdownTimer(knockBackTime);
            _knockbackTimer.OnComplete += FinishKnockBack;

            _coyoteTimer = new CountdownTimer(coyoteTime);
            _coyoteTimer.OnUpdate += CoyoteCheck;
        }

        private void OnValidate() {
            if (_jumpBufferTimer != null) _jumpBufferTimer.Duration = jumpBufferTime;
            if (_dashTimer != null) _dashTimer.Duration = dashTime;
            if (_minJumpTimer != null) _minJumpTimer.Duration = minJumpTime;
            if (_knockbackTimer != null) _knockbackTimer.Duration = knockBackTime;
            if (_coyoteTimer != null) _coyoteTimer.Duration = coyoteTime;
        }
        #endregion
    }
}