using System;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    protected bool isDead;
    protected bool isVulnerable;
    protected bool isStunned;
    protected Vector2 facingDirection;

    [SerializeField] private bool faceLeftOnStart;

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
