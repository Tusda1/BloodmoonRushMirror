using System;
using UnityEngine;

public class PatrolEnemy : EnemyBase
{
    private Collider2D collider;

    private new void OnEnable()
    {
        base.OnEnable();
        
        collider = GetComponent<Collider2D>();
    }
    
    private void Update()
    {
        if (isDead) { return; }

        if (isStunned) { return; }

        Patrol();
    }
    
    private void Patrol()
    {
        targetVelocity = facingDirection * moveSpeed;
        
        // Check if there is a wall in front of the enemy
        RaycastHit2D hit = Physics2D.Raycast(GetBottomPosition(), facingDirection, 1f, LayerMask.GetMask("Ground"));
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
    
    private Vector2 GetBottomPosition()
    {
        return new Vector2(collider.bounds.center.x, collider.bounds.min.y + 0.05f);
    }
}
