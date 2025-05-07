using System;
using System.Collections;
using Animancer;
using UnityEngine;
using Animancer.FSM;
using Player;
using VContainer;
#if STEAM_BUILD
using Steamworks;
#endif

[DefaultExecutionOrder(-1)]
public class BossBase : GravityObject, IDamageable {
    [SerializeField] private string steamStatus = "#status_boss_";
    
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
    public event Action OnBattlePause;
    public event Action OnBattleResume;
    public event Action TakenDamage;
    public event Action OnDeath;
    public event Action OnReset;
    
    [SerializeField] private AnimationClip[] takeDamageAnimation;
    [SerializeField] private bool turnWhileMoving = true;
    [SerializeField] private DamageInstance bossDamageInstance;
    [SerializeField] public SmoothDampScript smoothDampScript = new();
    
    public Collider2D bossCollider;
    
    [HideInInspector] public Vector2 facingDirection;
    [HideInInspector] public bool bossGrounded;
    [HideInInspector] public bool canTakeDamage = true;
    [HideInInspector] public bool canDoDamage = true;
    [HideInInspector] public bool isDead;
    private Material _whiteMaterial;
    private Material _defaultMaterial;
    private Vector2 initialPosition;
    private PlayerScript _player;

    [SerializeField] private string hurtSoundID;
    [Inject] private IAudioManager _audioManager;
    
    protected new void Awake()
    {
        base.Awake();
        _stateMachine.InitializeAfterDeserialize();
        initialPosition = transform.position;
        
        CurrentHealth = maxHealth[currentPhase];
        facingDirection = Vector2.left;
        _whiteMaterial = Resources.Load<Material>("Materials/WhiteMaterial");
        _defaultMaterial = GetComponent<SpriteRenderer>().material;
    }

    protected override void Start()
    {
        base.Start();
        
        _player = ServiceLocator.Get<PlayerScript>();
        _player.OnRespawn += ResetBoss;
    }

    private void OnEnable()
    {
        if(_player != null) _player.OnRespawn += ResetBoss;
    }

    private void OnDisable()
    {
        if(_player != null) _player.OnRespawn -= ResetBoss;
    }

    protected void LateUpdate()
    {
        bossGrounded = isGrounded;

        if (turnWhileMoving)
        {
            if (targetVelocity.x != 0) facingDirection = targetVelocity.x > 0 ? Vector2.right : Vector2.left;
        
            transform.localScale = new Vector3(-facingDirection.x, 1, 1);
        }
    }
    
    public void StartBattle()
    {
        OnBattleStart?.Invoke();
#if STEAM_BUILD
        if (steamStatus != string.Empty) {
            SteamFriends.SetRichPresence("status", steamStatus);
        }
#endif
    }

    public void EndBattle()
    {
        OnBattleEnd?.Invoke();
    }

    public void PauseBattle()
    {
        OnBattlePause?.Invoke();
    }
    
    public void ResumeBattle()
    {
        OnBattleResume?.Invoke();
    }
    
    public void EnableGravity()
    {
        GravityModifier = 1f;
    }
    
    public void DisableGravity()
    {
        GravityModifier = 0f;
    }

    public void Move(Vector2 moveVectorGoal)
    {
        smoothDampScript.SetStartVelocity(velocity);
        smoothDampScript.SmoothDampVector2(moveVectorGoal, ref targetVelocity);
    }
    
    public void MoveToNextPhase()
    {
        if (currentPhase + 1 == maxHealth.Length)
        {
            OnBattleEnd?.Invoke();
            return;
        }
        
        isDead = false;
        currentPhase++;
        CurrentHealth = maxHealth[currentPhase];
    }

    public void TakeDamage(DamageInstance damageInstance)
    {
        if (!canTakeDamage || isDead) return;
        
        CurrentHealth -= damageInstance.damage;

        if(!string.IsNullOrEmpty(hurtSoundID))_audioManager.PlayOneShotAttached(hurtSoundID, gameObject);
        StartCoroutine(FlashWhite());
        
        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }

        if (takeDamageAnimation.Length > 0)
        {
            var state = animancer.Play(takeDamageAnimation[currentPhase]);
            state.Events(this).OnEnd = () =>
            {
                state.Destroy();
                TakenDamage?.Invoke();
            };
        }
        else
        {
            TakenDamage?.Invoke();
        }
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
        isDead = true;
        OnDeath?.Invoke();
    }

    public void ResetBoss()
    {
        OnReset?.Invoke();
        isDead = false;
        currentPhase = 0;
        CurrentHealth = maxHealth[currentPhase];
        transform.position = initialPosition;
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
        animancer = GetComponent<AnimancerComponent>();
        bossCollider = GetComponent<Collider2D>();
    }
#endif
}