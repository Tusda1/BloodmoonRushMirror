using System;
using System.Collections;
using UnityEngine;

public class EnemyBase :  GravityObject, IDamageable
{
    protected bool isDead;
    protected bool isVulnerable;
    protected bool isStunned;
    protected Vector2 facingDirection;
    protected Collider2D boxCollider;

    [SerializeField] private bool faceLeftOnStart;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float maxHealth;
    protected float currentHealth;
    [SerializeField] protected float weight;
    
    protected LayerMask EnemyAndGroundLayerMask;

    protected new void Start()
    {
        base.Start();
        
        facingDirection = faceLeftOnStart ? Vector2.left : Vector2.right;
        
        EnemyAndGroundLayerMask = LayerMask.GetMask("Enemy", "Ground");
        
        boxCollider = GetComponent<Collider2D>();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void TakeDamage(DamageInstance damageInstance)
    {
        if (!isVulnerable) { return; }
        
        // Play hit animation
        // Play hit sound
        if (damageInstance.knockbackStrength.normalized == Vector2.up)
        {
            StartCoroutine(KnockUp(damageInstance.knockbackStrength));
        }
        else
        {
            StartCoroutine(Knockback(damageInstance.knockbackStrength));
        }
        
        currentHealth -= damageInstance.damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator KnockUp(Vector2 knockbackStrength)
    {
        isStunned = true;
        isVulnerable = true;
        float force = knockbackStrength.magnitude;
        velocity.y = force/weight;
        yield return new WaitUntil((() => velocity.y <= 0));
        yield return new WaitUntil((() => isGrounded));
        isStunned = false;
        isVulnerable = false;
    }
    
    private IEnumerator Knockback(Vector2 knockbackStrength)
    {
        isStunned = true;
        velocity.x = knockbackStrength.magnitude * knockbackStrength.normalized.x / weight;
        while (velocity.x != 0)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0, 3f);
            yield return null;
        }
        isStunned = false;
    }

    private void Die()
    {
        isDead = true;
        gameObject.SetActive(false);
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
