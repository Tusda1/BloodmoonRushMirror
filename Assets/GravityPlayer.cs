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
    private Vector2 inputVector;
    private Vector2 SmoothMovement;
    private Vector2 referenceSmoothVelocity;
    private bool isJumping = false;
    

    void Start()
    {
        gravityModifier = gravityMultiplier;
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
        
        SmoothMovement = Vector2.SmoothDamp(SmoothMovement, move, ref referenceSmoothVelocity, smoothTime);
        
        targetVelocity = SmoothMovement * maxSpeed;

        if (Mathf.Abs(GetVelocity().x) < 0.01f)
        {
            SmoothMovement.x = 0;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        print("jumping");
        if (context.performed)
            isJumping = true;
        if (context.canceled)
            isJumping = false;
    }

    public void Run(InputAction.CallbackContext context)
    {
        print("running");
        Vector2 referenceVector = context.ReadValue<Vector2>();
        inputVector.x = referenceVector.x;
    }
}
