using UnityEngine;
using Animancer.FSM;

[DefaultExecutionOrder(-10000)]
public class BossBase : GravityObject
{
    [SerializeField] private StateMachine<BossState>.WithDefault _stateMachine;
    public StateMachine<BossState>.WithDefault StateMachine => _stateMachine;
    
    protected new virtual void Awake()
    {
        base.Awake();
        _stateMachine.InitializeAfterDeserialize();
    }
}
