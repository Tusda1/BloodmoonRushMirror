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
        [Header("Player Movement")] [SerializeField, ReadOnly]
        private Vector2 inputVector;

        public bool isDucking;
        public bool isHoldingDuck;
        public bool isFacingRight = true;
        public bool isClimbing;
        [SerializeField] private float smoothTime;
        [SerializeField] private float maxSpeed;
        [SerializeField] private LayerMask whatIsLadder;
        [SerializeField] private LayerMask whatIsInvalidSpawnPoint;
        [SerializeField] private float climbSpeed;

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
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private bool isActuallyGrounded;

        [Header("Knockback")] public bool isKnockbacked;
        [SerializeField] private float knockBackTime;
        public int killCharges;

        [Header("Player Can Move")] [SerializeField, Required]
        private AllInFavourConsensusSO playerCanMove;

        private Vector2 knockbackVector;
        private bool PlayerMovementEnabled => playerCanMove.consensus;

        [Header("Debug Logger")] [SerializeField, Required]
        private DebugLoggerSO debugLogger;

        #region ***---PrivateFields---***
        [GetOrAddComponent] private BoxCollider2D _boxCollider;
        private CountdownTimer _dashTimer;
        private CountdownTimer _minJumpTimer;
        private CountdownTimer _jumpBufferTimer;
        private CountdownTimer _knockbackTimer;
        private CountdownTimer _coyoteTimer;
        private Vector2 _smoothMovement;
        private Vector2 _referenceSmoothVelocity;
        private Vector2 _lastSpikePoint;
        private bool _wasGroundedLastFrame;
        private Ladder _ladder;
        private bool _notClimbing;
        #endregion
        #endregion

        private new void Start() {
            GravityModifier = gravityMultiplier;

            InitializeTimers();
        }

        protected override void ComputeVelocity() {
            if (!PlayerMovementEnabled) return;

            if (LadderLogic()) return;

            GroundCheck();
            isActuallyGrounded = isGrounded;

            CoyoteTimeLogic();

            var move = Vector2.zero;

            if (isKnockbacked) {
                velocity.y = knockbackVector.y;
                targetVelocity.x = knockbackVector.x;
                _smoothMovement = Vector2.SmoothDamp(_smoothMovement, knockbackVector.normalized,
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
                doubleJumping = false;
            }
            else if (!isHoldingJump && !isJumping) {
                if (velocity.y > 0)
                    velocity.y *= fallSpeed;
            }
            else if (isJumping) {
                velocity.y = jumpForce;
            }

            if (isDashing) {
                targetVelocity.x = dashForce;
                _smoothMovement = Vector2.SmoothDamp(_smoothMovement, new Vector2(dashForce, 0).normalized,
                    ref _referenceSmoothVelocity, smoothTime);
            }
            else {
                if (isHoldingDuck && !isJumping && !isDucking && isGrounded) {
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

                targetVelocity = _smoothMovement * maxSpeed;

                isFacingRight = _smoothMovement.x switch {
                    > 0.01f => true,
                    < -0.01f => false,
                    _ => isFacingRight
                };
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

            RaycastHit2D hit = Physics2D.Raycast(quantizedPosition, Vector2.down, .5f);
            if (hit.collider != null && !hit.collider.CompareTag("Hazard") && hit.collider.CompareTag("Ground"))
                return true;
            return false;
        }

        #region ---EventFunctions---
        public void Jump(InputAction.CallbackContext context) {
            if (context.performed) {
                isHoldingJump = true;
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

            if (context.canceled) {
                isHoldingJump = false;
            }
        }

        public void Run(InputAction.CallbackContext context) {
            var referenceVector = context.ReadValue<Vector2>();
            inputVector = referenceVector.normalized;
        }

        public void Dash(InputAction.CallbackContext context) {
            if (!context.performed) return;
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

        public void TestFunction(InputAction.CallbackContext context) {
            killCharges += 1;
            Knockback(new Vector2(1, 0));
        }

        public void DuckInput(InputAction.CallbackContext context) {
            if (context.performed) {
                isHoldingDuck = true;
            }

            if (context.canceled) {
                isHoldingDuck = false;
            }
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
            //boxCollider.size = new Vector2(boxCollider.size.x, 2);
            gameObject.transform.localScale = new Vector3(1, 1, 1); //TEST
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
            //boxCollider.size = new Vector2(boxCollider.size.x, 2);
            gameObject.transform.localScale = new Vector3(1.5f, .5f, 1); //TEST
        }

        public void Knockback(Vector2 argumentKnockback) {
            isKnockbacked = true;

            _knockbackTimer.StartTimer();

            knockbackVector = argumentKnockback;
        }

        public void SpikeReset() {
            gameObject.transform.position = _lastSpikePoint;
            ResetMovement(0);
            isDashing = false;
        }

        #region ---Timers---
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
        }

        private void OnValidate() {
            if (_jumpBufferTimer != null) _jumpBufferTimer.Duration = jumpBufferTime;
            if (_dashTimer != null) _dashTimer.Duration = dashTime;
            if (_minJumpTimer != null) _minJumpTimer.Duration = minJumpTime;
            if (_knockbackTimer != null) _knockbackTimer.Duration = knockBackTime;
            if (_coyoteTimer != null) _coyoteTimer.Duration = coyoteTime;
        }
        #endregion


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