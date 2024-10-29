using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class EnemyBase :  GravityObject, IDamageable
{
    protected bool isDead;
    protected bool isVulnerable;
    protected bool isStunned;
    protected Vector2 facingDirection;
    protected Collider2D boxCollider;
    private GameObject _rubyPrefab;

    [SerializeField] private bool faceLeftOnStart;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float maxHealth;
    protected float currentHealth;
    [SerializeField] private DamageInstance enemyDamageInstance;
    [SerializeField] protected float weight;
    [SerializeField] protected int rubyDropAmount = 3;
    
    protected LayerMask EnemyAndGroundLayerMask;

    protected new void Start()
    {
        base.Start();
        
        facingDirection = faceLeftOnStart ? Vector2.left : Vector2.right;
        
        EnemyAndGroundLayerMask = LayerMask.GetMask("Enemy", "Ground");
        
        boxCollider = GetComponent<Collider2D>();
        _rubyPrefab = Resources.Load<GameObject>("Prefabs/Ruby");
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
        if (damageInstance.knockback.normalized == Vector2.up)
        {
            StartCoroutine(KnockUp(damageInstance.knockback));
        }
        else
        {
            StartCoroutine(Knockback(damageInstance.knockback));
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
        // Play death animation
        // Play death sound
        DropRubies();
        gameObject.SetActive(false);
    }
    
    private void DropRubies()
    {
        if (_rubyPrefab == null)
        {
            Debug.LogError("Ruby prefab not found");
            return;
        }

        //Drop as many rubies as the RubyDropAmount
        for (int i = 0; i < rubyDropAmount; i++)
        {
            // Generate a small random offset
            Vector3 offset = new Vector3(Random.Range(1.5f, 0.5f), Random.Range(0.5f, 0.5f), 0);
            // Add the offset to the enemy's position
            Vector3 spawnPosition = transform.position + offset;
            
            GameObject ruby = Instantiate(_rubyPrefab, spawnPosition, Quaternion.identity);
            ruby.GetComponent<RubyScript>().fall = true;
        }

        rubyDropAmount = 0;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isDead)
        {
            return;
        }

        if (isStunned)
        {
            return;
        }
        
        if (other.CompareTag("Player"))
        {
            IDamageable player = other.GetComponent<IDamageable>();
            if (player != null)
            {
                Vector2 directionToPlayer = (other.transform.position - transform.position).normalized;
                Vector2 knockbackDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;
                
                enemyDamageInstance.knockback = (knockbackDirection + Vector2.up * 0.5f).normalized * enemyDamageInstance.knockbackStrength;
                player.TakeDamage(enemyDamageInstance);
            }
        }
    }

    protected Vector2 GetBottomPosition()
    {
        return new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y + 0.05f);
    }
}
