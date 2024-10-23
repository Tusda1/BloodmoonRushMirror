using System;
using Attributes;
using Consensus;
using PlayerLoopTimer;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class GravityPlayer : GravityObject
{
    #region Variables
    
    [SerializeField] private float smoothTime;
    [SerializeField] private float fallSpeed;
    [SerializeField] private float gravityMultiplier;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashForce;
    [SerializeField] private float minJumpTime;
    [SerializeField] private float jumpBufferTime;
    [SerializeField] private float knockBackTime;
    public int killCharges;
    [SerializeField, Required] private AllInFavourConsensus playerCanMove;
    [GetOrAddComponent] private BoxCollider2D boxCollider;
    private CountdownTimer dashTimer;
    private CountdownTimer minJumpTimer;
    private CountdownTimer jumpBufferTimer;
    private CountdownTimer knockbackTimer;
    private Vector2 inputVector;
    private Vector2 SmoothMovement;
    private Vector2 referenceSmoothVelocity;
    public Vector2 knockbackVector;
    public bool isJumping = false;
    public bool isHoldingJump = false;
    public bool isDashing = false;
    public bool canDash = false;
    public bool isKnockbacked = false;
    public bool isDucking = false;
    public bool isHoldingDuck = false;
    private bool playerMovementEnabled => playerCanMove.consensus;
    public bool isFacingRight = true;

    void Start()
    {
        gravityModifier = gravityMultiplier;
        
        InitializeTimers();
    }
    
    #endregion

    #region ComputeVelocity
    
    protected override void ComputeVelocity()
    {
        Vector2 move = Vector2.zero;
        
        if (!playerMovementEnabled) return;

        if (isKnockbacked)
        {
            velocity.y = knockbackVector.y;
            targetVelocity.x = knockbackVector.x;
            SmoothMovement = Vector2.SmoothDamp(SmoothMovement, knockbackVector.normalized, ref referenceSmoothVelocity, smoothTime);
            
            return;
        }
        
        move.x = inputVector.x;

        if (isGrounded && isJumping)
        {
            velocity.y = jumpForce;
        }
        else if (!isHoldingJump && !isJumping)
        {
            if (velocity.y > 0)
                velocity.y = velocity.y * fallSpeed;
        }

        if (isDashing)
        {
            targetVelocity.x = dashForce;
            SmoothMovement = Vector2.SmoothDamp(SmoothMovement, new Vector2(dashForce, 0).normalized, ref referenceSmoothVelocity, smoothTime);
        }
        else
        {
            if (isGrounded) canDash = true;
            
            if (isHoldingDuck && !isJumping && !isDucking && isGrounded)
            {
                EnterDuck();
            }
            else if (isDucking && !isHoldingDuck)
            {
                ForceExitDuck();
            }

            if (isDucking)
            {
                if(inputVector.x > 0.01f) isFacingRight = true;
                else if(inputVector.x < -0.01f) isFacingRight = false;
                move = Vector2.zero;
            }
            
            SmoothMovement = Vector2.SmoothDamp(SmoothMovement, move, ref referenceSmoothVelocity, smoothTime);
        
            targetVelocity = SmoothMovement * maxSpeed;
        
            if(SmoothMovement.x > 0.01f) isFacingRight = true;
            else if(SmoothMovement.x < -0.01f) isFacingRight = false;
        }
        
        if (Mathf.Abs(GetVelocity().x) < 0.01f)
        {
            SmoothMovement.x = 0;
        }
    }
    
    #endregion

    #region Functions
    
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isHoldingJump = true;
            ForceExitDuck();
            if (isGrounded)
            {
                isJumping = true;
                minJumpTimer.StartTimer();
            }
            else
            {
                jumpBufferTimer.StartTimer();
            }
        }

        if (context.canceled)
        {
            isHoldingJump = false;
            // if (jumpBufferTimer.TimerRunning) 
            // {
            //     jumpBufferTimer.StopTimer();
            //     isJumping = false;
            // }
        }
    }

    public void Run(InputAction.CallbackContext context)
    {
        Vector2 referenceVector = context.ReadValue<Vector2>();
        inputVector.x = referenceVector.normalized.x;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isDashing || !canDash) return;

            ForceExitDuck();
            isDashing = true;
            canDash = false;
            lockToAxis = 1;
            dashTimer.StartTimer();
        
            if (isFacingRight)
                dashForce = Mathf.Abs(dashForce);
            else
                dashForce = -Mathf.Abs(dashForce);
        }
    }

    public void TestFunction(InputAction.CallbackContext context)
    {
        Knockback(new Vector2(1, 0), true);
    }

    public void DuckInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isHoldingDuck = true;
        }
        
        if (context.canceled)
        {
            isHoldingDuck = false;
        }
    }

    private void ForceExitDuck()
    {
        print("exiting duck");
        isDucking = false;
        //boxCollider.size = new Vector2(boxCollider.size.x, 2);
        gameObject.transform.localScale = new Vector3(1, 1, 1); //TEST
    }

    private void EnterDuck()
    {
        print("entering duck");
        isDucking = true;
        //boxCollider.size = new Vector2(boxCollider.size.x, 2);
        gameObject.transform.localScale = new Vector3(1.5f, .5f, 1); //TEST
    }

    public void Knockback(Vector2 enemyPosition, bool givesKnockback)
    {
        if (!givesKnockback) return;

        isKnockbacked = true;
        
        knockbackTimer.StartTimer();
        
        if (enemyPosition.x < transform.position.x)
        {
            knockbackVector.x = Mathf.Abs(knockbackVector.x);
        }
        else
        {
            knockbackVector.x = -Mathf.Abs(knockbackVector.x);
        }
    }
    
    #endregion

    #region Timers
    
    private void MinJumpCancel()
    {
        isJumping = false;
    }

    private void CompleteDash()
    {
        dashTimer.StopTimer();
        lockToAxis = 0;
        isDashing = false;
        ResetMovement(2);
    }

    private void JumpBuffer()
    {
        if (isGrounded)
        {
            isJumping = true;
            minJumpTimer.StartTimer();
            jumpBufferTimer.StopTimer();
        }
    }

    private void CompleteJumpBuffer()
    {
        isJumping = false;
    }

    private void FinishKnockBack()
    {
        isKnockbacked = false;
    }

    private void InitializeTimers()
    {
        dashTimer = new CountdownTimer(dashTime);
        dashTimer.OnComplete += CompleteDash;

        minJumpTimer = new CountdownTimer(minJumpTime);
        minJumpTimer.OnComplete += MinJumpCancel;

        jumpBufferTimer = new CountdownTimer(jumpBufferTime);
        jumpBufferTimer.OnUpdate += JumpBuffer;
        jumpBufferTimer.OnComplete += CompleteJumpBuffer;
        
        knockbackTimer = new CountdownTimer(knockBackTime);
        knockbackTimer.OnComplete += FinishKnockBack;
    }

    private void OnValidate()
    {
        if (jumpBufferTimer != null) jumpBufferTimer.Duration = jumpBufferTime;
        if (dashTimer != null) dashTimer.Duration = dashTime;
        if (minJumpTimer != null) minJumpTimer.Duration = minJumpTime;
        if (knockbackTimer != null) knockbackTimer.Duration = knockBackTime;
    }
    
    #endregion
}
