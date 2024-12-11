using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class EnemyBase :  GravityObject, IDamageable
{
    protected bool isDead;
    protected bool isVulnerable;
    protected bool isStunned;
    [SerializeField] private bool _takesSpikeDamage;
    private float _takeDamageTimer;
    protected Vector2 facingDirection;
    protected Vector2 visualFacingDirection;
    protected Collider2D boxCollider;
    private GameObject _rubyPrefab;
    private Material _whiteMaterial;
    private Material _defaultMaterial;
    protected EnemyAnimationBase enemyAnimationBase;
    protected PlayerScript player;

    [SerializeField] private bool faceLeftOnStart;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float maxHealth;
    protected float currentHealth;
    [SerializeField] private float invulnerableTime = 0.5f;
    [SerializeField] private float knockbackDuration = 0.1f;
    [SerializeField] private DamageInstance enemyDamageInstance;
    [SerializeField] protected float weight;
    [SerializeField] protected int rubyDropAmount = 3;
    [SerializeField] private SmoothDampScript knockbackSmoothDampScript = new();
    [SerializeField] private ParticleSystem bloodSplatter;
    
    protected LayerMask EnemyAndGroundLayerMask;
    
    protected override void Start()
    {
        base.Start();
        
        currentHealth = maxHealth;
        
        enemyAnimationBase = GetComponent<EnemyAnimationBase>();
        
        facingDirection = faceLeftOnStart ? Vector2.left : Vector2.right;
        
        EnemyAndGroundLayerMask = LayerMask.GetMask("Enemy", "Ground");

        player = ServiceLocator.Get<PlayerScript>();
        boxCollider = GetComponent<Collider2D>();
        _rubyPrefab = Resources.Load<GameObject>("Prefabs/Ruby");
        _whiteMaterial = Resources.Load<Material>("Materials/WhiteMaterial");
        _defaultMaterial = GetComponent<SpriteRenderer>().material;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    protected new void Update()
    {
        base.Update();
        
        SetVisualFacingDirection();
        // Set the localscale of the enemy to face the direction it is moving
        transform.localScale = new Vector3(-visualFacingDirection.x, 1, 1);
    }
    
    protected virtual void SetVisualFacingDirection()
    {
        visualFacingDirection = facingDirection;
    }

    public void TakeDamage(DamageInstance damageInstance)
    {
        print("taking damage");
        if (Time.time - _takeDamageTimer < invulnerableTime && !damageInstance.ignoreInvincibility)
        {
            return;
        }
        _takeDamageTimer = Time.time;
        
        print("taking damage 1");
        enemyAnimationBase.PlayHitAnimation();
        StartCoroutine(FlashWhite());

        if (bloodSplatter != null) {
            var deg = Vector2.Angle(damageInstance.knockback.normalized, Vector2.right);
            bloodSplatter.transform.rotation = new Quaternion(0, 0, deg, 0);
            bloodSplatter.transform.position = transform.position;
            bloodSplatter.Clear();
            bloodSplatter.Play();
        }
        else {
            Debug.LogWarning($"blood splatter on {name} is null");
        }
        
        print("taking damage 2");
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
        
        print("taking damage 3");
        currentHealth -= damageInstance.damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator FlashWhite()
    {
        // Flash white
        GetComponent<SpriteRenderer>().material = _whiteMaterial;
        yield return new WaitForSeconds(0.1f);
        // Return to normal
        GetComponent<SpriteRenderer>().material = _defaultMaterial;
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
        enemyAnimationBase.PlayDefaultAnimation();
    }
    
    private IEnumerator Knockback(Vector2 knockbackStrength)
    {
        isStunned = true;
        Vector2 knockbackVelocity = knockbackStrength.magnitude * knockbackStrength.normalized / weight;
        Vector2 smoothedVelocity = knockbackVelocity;
        smoothedVelocity.y = 0;
        float timer = 0f;

        while (timer < knockbackDuration)
        {
            targetVelocity = knockbackVelocity;
            velocity.y = knockbackVelocity.y;
            timer += Time.deltaTime;
            yield return null;
        }

        knockbackSmoothDampScript.SetStartVelocity(smoothedVelocity);
        while (!isGrounded)
        {
            // Smoothly come to a stop from knockback
            knockbackSmoothDampScript.SmoothDampFloat(knockbackVelocity.normalized, 0f, ref smoothedVelocity);
            targetVelocity = smoothedVelocity;
            yield return null;
        }

        isStunned = false;
        enemyAnimationBase.PlayDefaultAnimation();
    }

    private void Die()
    {
        isDead = true;
        
        enemyAnimationBase.PlayDeathAnimation(() =>
        {
            DropRubies();
            gameObject.SetActive(false);
        });
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
            bool acceptablePosition = false;
            Vector3 spawnPosition = Vector3.zero;
            
            while (acceptablePosition == false)
            {
                // Generate a small random offset
                Vector3 offset = new Vector3(Random.Range(1.5f, 0.5f), Random.Range(0.5f, 0.5f), 0);
                // Add the offset to the enemy's position
                spawnPosition = transform.position + offset;
                // Check if the position is valid
                Collider2D hit = Physics2D.OverlapCircle(spawnPosition, 0.2f, LayerMask.GetMask("Ground"));

                if (hit == null)
                {
                    acceptablePosition = true;
                }
            }
            
            GameObject ruby = Instantiate(_rubyPrefab, spawnPosition, Quaternion.identity);
            ruby.GetComponent<BetterRuby>().fall = true;
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
        
        if (player.healthComponent.HasInvincibilityFrames)
        {
            return;
        }
        
        if (other.CompareTag("Player"))
        {
            Attack();
        }
    }

    protected virtual void Attack()
    {
        var playerObject = player.gameObject;
        if (player != null)
        {
            Vector2 directionToPlayer = (playerObject.transform.position - transform.position).normalized;
            Vector2 knockbackDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;
                
            enemyDamageInstance.knockback = (knockbackDirection + Vector2.up * 0.5f).normalized * enemyDamageInstance.knockbackStrength;
            player.TakeDamage(enemyDamageInstance);
        }
    }

    protected Vector2 GetBottomPosition()
    {
        return new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y + 0.05f);
    }
}
