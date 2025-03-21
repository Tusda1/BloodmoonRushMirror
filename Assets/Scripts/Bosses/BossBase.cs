using System;
using System.Collections;
using Animancer;
using UnityEngine;
using Animancer.FSM;

[DefaultExecutionOrder(-10000)]
public class BossBase : GravityObject, IDamageable
{
    [SerializeField] private AnimancerComponent animancer;
    public AnimancerComponent AnimancerComponent => animancer;
    
    [SerializeField] private StateMachine<BossState>.WithDefault _stateMachine;
    public StateMachine<BossState>.WithDefault StateMachine => _stateMachine;

    [SerializeField] private int currentPhase;
    public int CurrentPhase => currentPhase;
    
    [SerializeField] private int[] maxHealth;
    public int[] MaxHealth => maxHealth;
    public int CurrentHealth {get; private set;}
    
    public event Action OnBattleStart;
    public event Action OnBattleEnd;
    public event Action TakenDamage;
    
    [SerializeField] private AnimationClip[] takeDamageAnimation;
    [SerializeField] private DamageInstance bossDamageInstance;
    
    [HideInInspector] public Vector2 facingDirection;
    [HideInInspector] public bool bossGrounded;
    [HideInInspector] public Collider2D bossCollider;
    [HideInInspector] public bool canTakeDamage = true;
    [HideInInspector] public bool canDoDamage = true;
    private Material _whiteMaterial;
    private Material _defaultMaterial;
    
    protected new void Awake()
    {
        base.Awake();
        _stateMachine.InitializeAfterDeserialize();
        
        CurrentHealth = maxHealth[currentPhase];
        facingDirection = Vector2.left;
        _whiteMaterial = Resources.Load<Material>("Materials/WhiteMaterial");
        _defaultMaterial = GetComponent<SpriteRenderer>().material;
    }
    
    protected override void Start()
    {
        base.Start();
        bossCollider = GetComponent<Collider2D>();
        
        StartCoroutine(LateActivation());
    }

    private IEnumerator LateActivation()
    {
        yield return new WaitForSeconds(1f);
        
        StartBattle();
    }

    protected void LateUpdate()
    {
        bossGrounded = isGrounded;
        
        if (targetVelocity.x != 0) facingDirection = targetVelocity.x > 0 ? Vector2.right : Vector2.left;
        
        transform.localScale = new Vector3(-facingDirection.x, 1, 1);
    }
    
    public void StartBattle()
    {
        OnBattleStart?.Invoke();
    }
    
    public void EnableGravity()
    {
        GravityModifier = 1f;
    }
    
    public void DisableGravity()
    {
        GravityModifier = 0f;
    }

    public void MoveToNextPhase()
    {
        if (currentPhase + 1 == maxHealth.Length)
        {
            OnBattleEnd?.Invoke();
            return;
        }
        
        currentPhase++;
        CurrentHealth = maxHealth[currentPhase];
    }

    public void TakeDamage(DamageInstance damageInstance)
    {
        if (!canTakeDamage) return;
        
        CurrentHealth -= damageInstance.damage;

        StartCoroutine(FlashWhite());
        
        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }
        
        var state = animancer.Play(takeDamageAnimation[currentPhase]);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            TakenDamage?.Invoke();
        };
    }
    
    private IEnumerator FlashWhite() {
        // Flash white
        GetComponent<SpriteRenderer>().material = _whiteMaterial;
        yield return new WaitForSeconds(0.1f);
        // Return to normal
        GetComponent<SpriteRenderer>().material = _defaultMaterial;
    }

    private void Die()
    {
        MoveToNextPhase();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDoDamage) return;
        
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>().TakeDamage(bossDamageInstance);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref animancer);
    }
#endif
}
