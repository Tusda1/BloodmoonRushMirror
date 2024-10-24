using System;
using UnityEngine;

public class EnemyBase :  GravityObject
{
    protected bool isDead;
    protected bool isVulnerable;
    protected bool isStunned;
    protected bool isKnockback;
    protected Vector2 facingDirection;
    protected Collider2D boxCollider;

    [SerializeField] private bool faceLeftOnStart;
    [SerializeField] protected float moveSpeed;
    
    protected LayerMask EnemyAndGroundLayerMask;

    protected void Initialize()
    {
        facingDirection = faceLeftOnStart ? Vector2.left : Vector2.right;
        
        EnemyAndGroundLayerMask = LayerMask.GetMask("Enemy", "Ground");
        
        boxCollider = GetComponent<Collider2D>();
    }

    private void Die()
    {
        isDead = true;
        // Play death animation
        // Play death sound
        // Drop loot
        // Destroy game object
    }
    
    protected Vector2 GetBottomPosition()
    {
        return new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y + 0.05f);
    }
}
