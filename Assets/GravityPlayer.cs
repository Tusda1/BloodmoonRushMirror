using System;
using PlayerLoopTimer;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
    private CountdownTimer dashTimer;
    private CountdownTimer minJumpTimer;
    private CountdownTimer jumpBufferTimer;
    private Vector2 inputVector;
    private Vector2 SmoothMovement;
    private Vector2 referenceSmoothVelocity;
    public bool isJumping = false;
    public bool isHoldingJump = false;
    public bool isDashing = false;
    public bool canDash = false;
    [SerializeField] bool isFacingRight = true;

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

    #region Input Functions
    
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isHoldingJump = true;
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

    private void InitializeTimers()
    {
        dashTimer = new CountdownTimer(dashTime);
        dashTimer.OnComplete += CompleteDash;

        minJumpTimer = new CountdownTimer(minJumpTime);
        minJumpTimer.OnComplete += MinJumpCancel;

        jumpBufferTimer = new CountdownTimer(jumpBufferTime);
        jumpBufferTimer.OnUpdate += JumpBuffer;
        jumpBufferTimer.OnComplete += CompleteJumpBuffer;
    }

    private void OnValidate()
    {
        if (jumpBufferTimer != null) jumpBufferTimer.Duration = jumpBufferTime;
        if (dashTimer != null) dashTimer.Duration = dashTime;
        if (minJumpTimer != null) minJumpTimer.Duration = minJumpTime;
    }
    
    #endregion
}
