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
    
    protected new virtual void Awake()
    {
        base.Awake();
        print("!!");
        _stateMachine.InitializeAfterDeserialize();
    }
    
    protected new virtual void Start()
    {
        base.Start();
        StartCoroutine(LateTeleport());
    }

    private IEnumerator LateTeleport()
    {
        yield return new WaitForSeconds(1);
        _stateMachine.ForceSetState(_stateMachine.DefaultState);
    }
}
