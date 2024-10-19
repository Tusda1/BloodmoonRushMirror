using PlayerLoopTimer;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GravityPlayer : GravityObject
{
    [SerializeField] private float smoothTime;
    [SerializeField] private float fallSpeed;
    [SerializeField] private float gravityMultiplier;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashForce;
    private CountdownTimer dashTimer;
    private Vector2 inputVector;
    private Vector2 SmoothMovement;
    private Vector2 referenceSmoothVelocity;
    public bool isJumping = false;
    public bool isDashing = false;
    public bool canDash = false;
    [SerializeField] bool isFacingRight = true;

    void Start()
    {
        gravityModifier = gravityMultiplier;
        
        dashTimer = new CountdownTimer(dashTime);
        dashTimer.OnComplete += CompleteDash;
    }
    
    protected override void ComputeVelocity()
    {
        Vector2 move = Vector2.zero;
        
        move.x = inputVector.x;

        if (isGrounded && isJumping)
        {
            velocity.y = jumpForce;
        }
        else if (!isJumping)
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

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
            isJumping = true;
        if (context.canceled)
            isJumping = false;
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

    private void CompleteDash()
    {
        dashTimer.StopTimer();
        lockToAxis = 0;
        isDashing = false;
        ResetMovement(2);
    }
}
