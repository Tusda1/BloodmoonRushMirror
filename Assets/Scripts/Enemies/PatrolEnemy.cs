using System;
using UnityEngine;

public class PatrolEnemy : EnemyBase
{
    [SerializeField] private float smoothTime = 0.1f;
    
    private Vector2 currentVelocity;
    private Vector2 smoothVelocity;
    
    private new void Update()
    {
        base.Update();
        
        if (isDead) { return; }

        if (isStunned) { return; }

        Patrol();
    }
    
    private void Patrol()
    {
        // Smoothly move towards the target
        smoothVelocity = Vector2.SmoothDamp(smoothVelocity, facingDirection * moveSpeed, ref currentVelocity, smoothTime);
        targetVelocity = smoothVelocity;
        
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
