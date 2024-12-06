using System;
using System.Collections;
using Animancer;
using UnityEngine;
using Animancer.FSM;

[DefaultExecutionOrder(-10000)]
public class BossBase : GravityObject
{
    [SerializeField] private AnimancerComponent animancer;
    public AnimancerComponent AnimancerComponent => animancer;
    
    [SerializeField] private StateMachine<BossState>.WithDefault _stateMachine;
    public StateMachine<BossState>.WithDefault StateMachine => _stateMachine;

    [SerializeField] private AnimationClip teleportEnter;
    public AnimationClip TeleportEnter => teleportEnter;
    [SerializeField] private AnimationClip teleportExit;
    public AnimationClip TeleportExit => teleportExit;
    [SerializeField] private int maxHealth;
    public int MaxHealth => maxHealth;
    public int CurrentHealth {get; private set;}
    
    public event Action OnBattleStart;
    
    [HideInInspector] public float targetVelocityX;
    [HideInInspector] public bool bossGrounded;
    [HideInInspector] public Collider2D bossCollider;
    
    protected new void Awake()
    {
        base.Awake();
        print("!!");
        _stateMachine.InitializeAfterDeserialize();
        
        CurrentHealth = maxHealth;
    }
    
    protected override void Start()
    {
        base.Start();
        bossCollider = GetComponent<Collider2D>();
        
        StartCoroutine(LateActivation());
    }

    private IEnumerator LateActivation()
    {
        yield return new WaitForSeconds(1);
        OnBattleStart?.Invoke();
    }

    protected override void ComputeVelocity()
    {
        targetVelocity.x = targetVelocityX;
        bossGrounded = isGrounded;
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
}
