using ExtensionMethods;
using Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using static GravityObject;

public class PlayerLadderLogic : IPlayerLadderLogic {
    #region ---Fields---
    private readonly PlayerScript _player;
    private readonly GravityObjectPlayerMovement _playerMovement;
    private readonly DebugLoggerSO _debugLogger;
        
    public LadderTop LadderTop { get; set; }
    public LadderMiddle LadderMiddle {
        get {
            if (_ladderMiddle == null && LadderTop != null && InputVector.y < 0) {
                _ladderMiddle = LadderTop.ladderMiddle;
            }

            return _ladderMiddle;
        }
        set => _ladderMiddle = value;
    }
    
    private LadderMiddle _ladderMiddle;
    public LadderBottom LadderBottom { get; set; }

    #region ---PlayerMovementShortcuts---
    private bool IsClimbing {
        get => _playerMovement.isClimbing;
        set => _playerMovement.isClimbing = value;
    }

    private Vector2 InputVector => _playerMovement.inputVector;

    private float ClimbSpeed => _playerMovement.climbSpeed;

    private bool IsGrounded {
        get => _playerMovement.isGrounded;
        set => _playerMovement.isGrounded = value;
    }

    private float GravityModifier {
        set => _playerMovement.GravityModifier = value;
    }

    private float GravityMultiplier => _playerMovement.gravityMultiplier;

    private readonly Transform _transform;

    private bool IsHoldingJump => _playerMovement.isHoldingJump;
    private bool IsDashing => _playerMovement.isDashing;

    private LayerMask WhatIsLadder => _playerMovement.whatIsLadder;
    #endregion

    #region ---AnimatorStringToHash---
    private static readonly int AnimatorYMovementFloat = Animator.StringToHash("YMovement");
    private static readonly int AnimatorClimbBool = Animator.StringToHash("IsClimbing");

    private static readonly int AnimatorClimbTopExitTrigger = Animator.StringToHash("LadderExitAtTopTrigger");
    private static readonly int AnimatorClimbEnterTopTrigger = Animator.StringToHash("LadderEnterTopTrigger");
    #endregion
    #endregion

    public PlayerLadderLogic(PlayerScript player, DebugLoggerSO debugLogger) {
        _player = player;
        _debugLogger = debugLogger;
        _playerMovement = player.playerMovement;
        _transform = player.transform;
        Assert.IsNotNull(_transform);
    }

    /// <summary>
    /// Called in update
    /// </summary>
    /// <returns>True stops default movement update from running</returns>
    public bool LadderLogic() {
        if (!IsClimbing) {
            if (InputVector.y != 0 && LadderMiddle != null &&
                !(InputVector.y < 0 && _playerMovement.isActuallyGrounded) && !_playerMovement.isHoldingJump) {
                EnterLadder();
            }
            else {
                return false;
            }
        }

        if (LadderMiddle == null && LadderBottom == null && LadderTop == null) {
            ExitLadder();
        }

        IsGrounded = IsClimbing;

        _playerMovement.targetVelocity = new Vector2(0, InputVector.y * ClimbSpeed);

        if (InputVector.y > 0) {
            if (LadderTop) {
                ClimbToTopOfLadder(LadderTop.ladderTopPosition.position);

                ExitLadder();
            }

            if ((IsGrounded && InputVector.y < 0) || IsHoldingJump || IsDashing) {
                IsClimbing = false;
                GravityModifier = GravityMultiplier;
                // LadderMiddle = null;
            }
            else {
                IsGrounded = true;
                _playerMovement.ForceExitDuck();
            }
        }
        else if (InputVector.y < 0) {
            if (_playerMovement.isActuallyGrounded) {
                ExitLadder();
            }
        }

        _player.animator.SetFloat(AnimatorYMovementFloat, InputVector.y);

        return true;
    }

    private void ClimbToTopOfLadder(Vector3 ladderTopPosition) {
        _transform.position = ladderTopPosition;
        _player.animator.SetTrigger(AnimatorClimbTopExitTrigger);
        // _player.healthComponent.TriggerInvulnerability(0.2f);
        ExitLadder();
    }

    /// <summary>
    /// Called when ducking
    /// </summary>
    /// <param name="ladderTop">The ladder we are standing on if we are</param>
    /// <returns>True if standing on top of a ladder</returns>
    public bool TryGetLadderTop(out LadderTop ladderTop) {
        _debugLogger.Log("TryGetLadder");

        var point = _transform.position.WithOffset(_transform.localScale.y * 0.5f);
        var size = _transform.localScale.Abs();
        var ladder = Physics2D.OverlapBox(point, size, 0, WhatIsLadder);

        if (ladder != null) {
            if (!ladder.TryGetComponent(out ladderTop)) return false;
            EnterLadderFromTop();
            LadderMiddle = ladderTop.ladderMiddle;
            LadderTop = ladderTop;
            return true;
        }

        ladderTop = null;
        return false;
    }

    public void EnterLadder() {
        if (IsClimbing) return;
        
        _debugLogger.Log("EnterLadder");

        GravityModifier = 0;

        _playerMovement.ResetMovement(Axis.YAxis);

        _playerMovement.DashTimer.StopTimer();

        IsClimbing = true;
        IsGrounded = true;
        _playerMovement.lockToAxis = Axis.YAxis;
        _playerMovement.canDash = true;

        _transform.position = new Vector3(
            x: LadderMiddle.transform.position.x + 0.5f,
            y: _transform.position.y,
            z: _transform.position.z);

        _player.animator.SetBool(AnimatorClimbBool, true);
    }

    public void ExitLadder() {
        if (!IsClimbing) return;
        
        _debugLogger.Log("ExitLadder");

        GravityModifier = GravityMultiplier;

        IsClimbing = false;
        IsGrounded = true;
        _playerMovement.lockToAxis = Axis.None;

        _player.animator.SetBool(AnimatorClimbBool, false);
    }

    private void EnterLadderFromTop() {
        _debugLogger.Log("Enter ladder from top");

        EnterLadder();

        _transform.position = LadderTop.ladderTopBottom.position;
        _player.animator.SetTrigger(AnimatorClimbEnterTopTrigger);
    }
}