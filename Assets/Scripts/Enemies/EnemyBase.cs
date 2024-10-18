using System;
using UnityEngine;

public class EnemyBase :  GravityObject
{
    protected bool isDead;
    protected bool isVulnerable;
    protected bool isStunned;
    protected Vector2 facingDirection;

    [SerializeField] private bool faceLeftOnStart;
    [SerializeField] protected float moveSpeed;

    private void Start()
    {
        facingDirection = faceLeftOnStart ? Vector2.left : Vector2.right;
    }

    private void Die()
    {
        isDead = true;
        // Play death animation
        // Play death sound
        // Drop loot
        // Destroy game object
    }
}
