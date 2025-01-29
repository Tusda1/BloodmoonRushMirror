using System;
using System.Collections;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Utilities.DataBinding;

public class EnemyBase : GravityObject, IDamageable, IBind<EnemyData> {
    #region ---Fields---
    #region ***---SerializedFields---***
    public bool takesSpikeDamage;
    [SerializeField] private bool faceLeftOnStart;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected int maxHealth;
    [SerializeField] private float invulnerableTime = 0.5f;
    [SerializeField] private float knockbackDuration = 0.1f;
    [SerializeField] private DamageInstance enemyDamageInstance;
    [SerializeField] protected float weight = 1;
    [SerializeField] protected int rubyDropAmount = 3;
    [SerializeField] private float rubySpawnHeight = 0.8f;
    [SerializeField] private float rubySpawnDelay = 0.2f;
    [SerializeField] private SmoothDampScript knockbackSmoothDampScript = new();
    [SerializeField] private ParticleSystem bloodSplatter;
    [SerializeField] private bool isImmortal;
    [SerializeField] private bool canTakeKnockback = true;
    #endregion

    #region ***---PrivateFields---***
    protected Vector2 startPosition;
    protected Vector2 facingDirection;
    protected Vector2 visualFacingDirection;
    
    protected bool isDead;
    protected bool isStunned;
    protected bool isVulnerable;
    protected int currentHealth = 2;
    protected float takeDamageTimer;
    protected LayerMask enemyAndGroundLayerMask;
    protected Collider2D enemyCollider;
    
    protected EnemyAnimationBase enemyAnimationBase;
    protected PlayerScript player;
    
    private GameObject _rubyPrefab;
    private Material _whiteMaterial;
    private Material _defaultMaterial;
    private Pooler _rubyPooler;
    #endregion
    
    #region ***---GameManagers---***
    private static SaveLoadSystemManager SaveLoadSystemManager => SaveLoadSystemManager.Instance;
    private static LevelDataManager LevelDataManager => LevelDataManager.LevelDataManagerCache;
    #endregion

    #region ***---DataBindingVariables---***
    [field: Header("Data Binding")]
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public EnemyData SaveSystemData { get; set; }
    public EnemyData DataInGameData => LevelDataManager.SaveSystemData.enemyDatas.GetOrAddByIdInList(ID,
        new EnemyData {
            name = name,
            ID = ID,
            health = currentHealth,
            gemsContaining = rubyDropAmount,
            isAlive = !isDead,
            position = transform.position
        });
    #endregion
    #endregion

    #region ---Methods---
    protected override void Start() {
        base.Start();

        startPosition = transform.position;

        currentHealth = maxHealth;

        enemyAnimationBase = GetComponent<EnemyAnimationBase>();

        facingDirection = faceLeftOnStart ? Vector2.left : Vector2.right;

        enemyAndGroundLayerMask = LayerMask.GetMask("Enemy", "Ground");

        player = ServiceLocator.Get<PlayerScript>();
        enemyCollider = GetComponent<Collider2D>();
        _rubyPrefab = Resources.Load<GameObject>("Prefabs/Ruby");
        _whiteMaterial = Resources.Load<Material>("Materials/WhiteMaterial");
        _defaultMaterial = GetComponent<SpriteRenderer>().material;
        
        _rubyPooler = TryGetCommunityPooler.TryGetPooler(_rubyPrefab);
        
        Bind();
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    protected new void Update() {
        base.Update();

        SetVisualFacingDirection();
        // Set the localscale of the enemy to face the direction it is moving
        transform.localScale = new Vector3(-visualFacingDirection.x, 1, 1);
    }

    protected virtual void SetVisualFacingDirection() {
        visualFacingDirection = facingDirection;
    }

    public void TakeDamage(DamageInstance damageInstance) {
        if (!CanBeDamaged(damageInstance)) {
            return;
        }
        
        print("taking damage");
        if (Time.time - takeDamageTimer < invulnerableTime && !damageInstance.ignoreInvincibility) {
            return;
        }

        takeDamageTimer = Time.time;

        if (enemyAnimationBase != null) enemyAnimationBase.PlayHitAnimation();
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

        if (!isImmortal) currentHealth -= damageInstance.damage;

        if (currentHealth <= 0) {
            Die();
            return;
        }

        // Play hit animation
        // Play hit sound
        if (canTakeKnockback) StartCoroutine(Knockback(damageInstance.knockback));
    }
    
    protected virtual bool CanBeDamaged(DamageInstance damageInstance)
    {
        return true;
    }

    private IEnumerator FlashWhite() {
        // Flash white
        GetComponent<SpriteRenderer>().material = _whiteMaterial;
        yield return new WaitForSeconds(0.1f);
        // Return to normal
        GetComponent<SpriteRenderer>().material = _defaultMaterial;
    }

    private IEnumerator KnockUp(Vector2 knockbackStrength) {
        isStunned = true;
        isVulnerable = true;
        float force = knockbackStrength.magnitude;
        velocity.y = force / weight;
        yield return new WaitUntil((() => velocity.y <= 0));
        yield return new WaitUntil((() => isGrounded));
        if (isDead) yield break;
        isStunned = false;
        isVulnerable = false;
        enemyAnimationBase.PlayDefaultAnimation();
    }

    private IEnumerator Knockback(Vector2 knockbackStrength) {
        isStunned = true;
        Vector2 knockbackVelocity = knockbackStrength.magnitude * knockbackStrength.normalized / weight;
        Vector2 smoothedVelocity = knockbackVelocity;
        smoothedVelocity.y = 0;
        float timer = 0f;

        while (timer < knockbackDuration) {
            targetVelocity = knockbackVelocity;
            velocity.y = knockbackVelocity.y;
            timer += Time.deltaTime;
            yield return null;
        }

        knockbackSmoothDampScript.SetStartVelocity(smoothedVelocity);
        while (!isGrounded) {
            // Smoothly come to a stop from knockback
            knockbackSmoothDampScript.SmoothDampVector2(Vector2.zero, ref smoothedVelocity);
            targetVelocity = smoothedVelocity;
            yield return null;
        }

        if (isDead) yield break;

        isStunned = false;
        if (enemyAnimationBase != null) enemyAnimationBase.PlayDefaultAnimation();
    }

    private void Die() 
    {
        isDead = true;

        if (enemyAnimationBase != null) enemyAnimationBase.PlayDeathAnimation(rubySpawnDelay, DropRubies, OnDeath);
    }

    protected virtual void OnDeath()
    {
        gameObject.SetActive(false);
    }

    private void DropRubies() {
        if (_rubyPrefab == null) {
            Debug.LogError("Ruby prefab not found");
            return;
        }

        Vector3 positionBase = GetBottomPosition() + Vector2.up * rubySpawnHeight;

        //Drop as many rubies as the RubyDropAmount
        for (int i = 0; i < rubyDropAmount; i++) {
            bool acceptablePosition = false;
            Vector3 spawnPosition = Vector3.zero;
            int attempts = 0;

            while (acceptablePosition == false) {
                attempts++;
                // Generate a small random offset
                var offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.4f, 0.6f), 0);
                // Add the offset to the enemy's position
                spawnPosition = positionBase + offset;
                // Check if the position is valid
                var hit = Physics2D.OverlapCircle(spawnPosition, 0.4f, LayerMask.GetMask("Ground"));

                if (hit == null) {
                    acceptablePosition = true;
                }

                if (attempts > 10) {
                    acceptablePosition = true;
                }
            }

            var rubyObject = _rubyPooler.GetPooledObject();
            rubyObject.transform.position = spawnPosition;
            rubyObject.SetActive(true);
            var betterRuby = rubyObject.GetComponent<BetterRuby>();
            betterRuby.fall = true;
            betterRuby.enabled = true;
        }

        rubyDropAmount = 0;
    }

    public void Respawn() {
        isDead = false;
        isVulnerable = false;
        isStunned = false;
        gameObject.SetActive(true);
        currentHealth = maxHealth;
        transform.position = startPosition;
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (isDead) {
            return;
        }

        if (isStunned) {
            return;
        }

        if (player.healthComponent.HasInvincibilityFrames) {
            return;
        }

        if (other.CompareTag("Player")) {
            Attack();
        }
    }

    protected virtual void Attack() {
        var playerObject = player.gameObject;
        if (player != null) {
            Vector2 directionToPlayer = (playerObject.transform.position - transform.position).normalized;
            Vector2 knockbackDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;

            enemyDamageInstance.knockback = (knockbackDirection + Vector2.up * 0.5f).normalized * enemyDamageInstance.knockbackStrength;
            player.TakeDamage(enemyDamageInstance);
        }
    }

    protected Vector2 GetBottomPosition() {
        return new Vector2(enemyCollider.bounds.center.x, enemyCollider.bounds.min.y + 0.05f);
    }
    #endregion

#if UNITY_EDITOR
    private void OnValidate() {
        if (gameObject.activeInHierarchy && string.IsNullOrEmpty(ID)) {
            ID = Guid.NewGuid().ToString();
            Debug.Log($"Setting new guid for {name}");
        }

        currentHealth = maxHealth;
    }
#endif

    #region ---DataBindingMethods---
    public virtual void Bind() {
        if (SaveLoadSystemManager.HasInstance || !Application.isPlaying) {
            SaveLoadSystemManager.Bind(this, DataInGameData);
        }
        else {
            Debug.LogWarning("No SaveLoadSystemManager found", this);
        }
    }
    
    [ContextMenu(nameof(LoadBindData))]
    public virtual void LoadBindData() {
        name = SaveSystemData.name;
        currentHealth = SaveSystemData.health;
        rubyDropAmount = SaveSystemData.gemsContaining;
        isDead = !SaveSystemData.isAlive;
        transform.position = SaveSystemData.position;
    }

    [ContextMenu(nameof(SaveBindData))]
    public virtual void SaveBindData() {
        SaveSystemData.name = name;
        SaveSystemData.health = currentHealth;
        SaveSystemData.gemsContaining = rubyDropAmount;
        SaveSystemData.isAlive = !isDead;
        SaveSystemData.position = transform.position;
    }
    #endregion
}