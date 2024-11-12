using System;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolEnemy : EnemyBase
{
    [SerializeField] private SmoothDampScript smoothDampScript = new();
    
    private new void Update()
    {
        base.Update();
        
        if (isDead || isStunned || !isGrounded) { return; }

        Patrol();
    }
    
    private void Patrol()
    {
        // Smoothly move towards the target
        smoothDampScript.SmoothDamp(facingDirection, moveSpeed, ref targetVelocity);
        
        // Check if there is a wall in front of the enemy
        RaycastHit2D hit = Physics2D.Raycast(GetBottomPosition(), facingDirection, 1f, EnemyAndGroundLayerMask);
        if (hit.collider != null)
        {
            // Change direction
            facingDirection = -facingDirection;
        }
        
        // Check if there is a ledge in front of the enemy
        hit = Physics2D.Raycast(GetBottomPosition() + facingDirection, Vector2.down, 1f, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            // Change direction
            facingDirection = -facingDirection;
        }
    }
}
