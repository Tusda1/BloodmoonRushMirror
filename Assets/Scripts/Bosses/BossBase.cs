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

    [HideInInspector] public float gravity;
    [HideInInspector] public float targetVelocityX;
    
    protected new virtual void Awake()
    {
        base.Awake();
        print("!!");
        _stateMachine.InitializeAfterDeserialize();
    }
    
    protected new virtual void Start()
    {
        base.Start();
        StartCoroutine(LateActivation());
    }

    private IEnumerator LateActivation()
    {
        yield return new WaitForSeconds(1);
        _stateMachine.ForceSetState(_stateMachine.DefaultState);
    }

    private new void Update()
    {
        base.Update();
        targetVelocity.x = targetVelocityX;
        GravityModifier = gravity;
    }
}
