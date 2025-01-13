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
    [SerializeField] private bool takesSpikeDamage;
    [SerializeField] private bool faceLeftOnStart;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected int maxHealth;
    [SerializeField] private float invulnerableTime = 0.5f;
    [SerializeField] private float knockbackDuration = 0.1f;
    [SerializeField] private DamageInstance enemyDamageInstance;
    [SerializeField] protected float weight;
    [SerializeField] protected int rubyDropAmount = 3;
    [SerializeField] private float rubySpawnHeight = 0.8f;
    [SerializeField] private float rubySpawnDelay = 0.2f;
    [SerializeField] private SmoothDampScript knockbackSmoothDampScript = new();
    [SerializeField] private ParticleSystem bloodSplatter;
    #endregion

    #region ***---PrivateFields---***
    protected Vector2 StartPosition;
    protected Vector2 FacingDirection;
    protected Vector2 VisualFacingDirection;
    
    protected bool IsDead;
    protected bool IsStunned;
    protected bool IsVulnerable;
    protected int CurrentHealth = 2;
    protected float TakeDamageTimer;
    protected LayerMask EnemyAndGroundLayerMask;
    
    protected EnemyAnimationBase enemyAnimationBase;
    protected PlayerScript Player;
    
    private GameObject _rubyPrefab;
    private Material _whiteMaterial;
    private Material _defaultMaterial;
    private Collider2D _boxCollider;
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
            health = CurrentHealth,
            gemsContaining = rubyDropAmount,
            isAlive = !IsDead,
            position = transform.position
        });
    #endregion
    #endregion

    #region ---Methods---
    protected override void Start() {
        base.Start();

        StartPosition = transform.position;

        CurrentHealth = maxHealth;

        enemyAnimationBase = GetComponent<EnemyAnimationBase>();

        FacingDirection = faceLeftOnStart ? Vector2.left : Vector2.right;

        EnemyAndGroundLayerMask = LayerMask.GetMask("Enemy", "Ground");

        Player = ServiceLocator.Get<PlayerScript>();
        _boxCollider = GetComponent<Collider2D>();
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
        transform.localScale = new Vector3(-VisualFacingDirection.x, 1, 1);
    }

    protected virtual void SetVisualFacingDirection() {
        VisualFacingDirection = FacingDirection;
    }

    public void TakeDamage(DamageInstance damageInstance) {
        print("taking damage");
        if (Time.time - TakeDamageTimer < invulnerableTime && !damageInstance.ignoreInvincibility) {
            return;
        }
        
        if (!CanBeDamaged(damageInstance)) {
            return;
        }

        TakeDamageTimer = Time.time;

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

        CurrentHealth -= damageInstance.damage;

        if (CurrentHealth <= 0) {
            Die();
            return;
        }

        // Play hit animation
        // Play hit sound
        print(damageInstance.knockback.normalized);
        if (damageInstance.knockback.normalized == Vector2.up) {
            StartCoroutine(KnockUp(damageInstance.knockback));
        }
        else {
            StartCoroutine(Knockback(damageInstance.knockback));
        }
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
        IsStunned = true;
        IsVulnerable = true;
        float force = knockbackStrength.magnitude;
        velocity.y = force / weight;
        yield return new WaitUntil((() => velocity.y <= 0));
        yield return new WaitUntil((() => isGrounded));
        if (IsDead) yield break;
        IsStunned = false;
        IsVulnerable = false;
        enemyAnimationBase.PlayDefaultAnimation();
    }

    private IEnumerator Knockback(Vector2 knockbackStrength) {
        IsStunned = true;
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
            knockbackSmoothDampScript.SmoothDampFloat(knockbackVelocity.normalized, 0f, ref smoothedVelocity);
            targetVelocity = smoothedVelocity;
            yield return null;
        }

        if (IsDead) yield break;

        IsStunned = false;
        if (enemyAnimationBase != null) enemyAnimationBase.PlayDefaultAnimation();
    }

    private void Die() {
        IsDead = true;

        if (enemyAnimationBase != null) enemyAnimationBase.PlayDeathAnimation(rubySpawnDelay, DropRubies, () => { gameObject.SetActive(false); });
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
        IsDead = false;
        IsVulnerable = false;
        IsStunned = false;
        gameObject.SetActive(true);
        CurrentHealth = maxHealth;
        transform.position = StartPosition;
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (IsDead) {
            return;
        }

        if (IsStunned) {
            return;
        }

        if (Player.healthComponent.HasInvincibilityFrames) {
            return;
        }

        if (other.CompareTag("Player")) {
            Attack();
        }
    }

    protected virtual void Attack() {
        var playerObject = Player.gameObject;
        if (Player != null) {
            Vector2 directionToPlayer = (playerObject.transform.position - transform.position).normalized;
            Vector2 knockbackDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;

            enemyDamageInstance.knockback = (knockbackDirection + Vector2.up * 0.5f).normalized * enemyDamageInstance.knockbackStrength;
            Player.TakeDamage(enemyDamageInstance);
        }
    }

    protected Vector2 GetBottomPosition() {
        return new Vector2(_boxCollider.bounds.center.x, _boxCollider.bounds.min.y + 0.05f);
    }
    #endregion

#if UNITY_EDITOR
    private void OnValidate() {
        if (gameObject.activeInHierarchy && string.IsNullOrEmpty(ID)) {
            ID = Guid.NewGuid().ToString();
            Debug.Log($"Setting new guid for {name}");
        }

        CurrentHealth = maxHealth;
    }
#endif

    #region ---DataBindingMethods---
    public virtual void Bind() {
        if (SaveLoadSystemManager.HasInstance) {
            SaveLoadSystemManager.Bind(this, DataInGameData);
        }
        else {
            Debug.LogWarning("No SaveLoadSystemManager found");
        }
    }
    
    [ContextMenu(nameof(LoadBindData))]
    public virtual void LoadBindData() {
        name = SaveSystemData.name;
        CurrentHealth = SaveSystemData.health;
        rubyDropAmount = SaveSystemData.gemsContaining;
        IsDead = !SaveSystemData.isAlive;
        transform.position = SaveSystemData.position;
    }

    [ContextMenu(nameof(SaveBindData))]
    public virtual void SaveBindData() {
        SaveSystemData.name = name;
        SaveSystemData.health = CurrentHealth;
        SaveSystemData.gemsContaining = rubyDropAmount;
        SaveSystemData.isAlive = !IsDead;
        SaveSystemData.position = transform.position;
    }
    #endregion
}