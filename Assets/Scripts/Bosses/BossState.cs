using Animancer;
using Animancer.FSM;
using UnityEngine;
using System;
using UnityEngine.Serialization;

public enum BossStatePriority
{
    Low,
    Medium,
    High
}

public class BossState : StateBehaviour
{
    [SerializeField] private BossBase boss;
    protected BossBase Boss => boss;

    [SerializeField] private int statePhase = 1;
    public int StatePhase => statePhase;
    
    public virtual BossStatePriority Priority => BossStatePriority.Low;

    public virtual bool CanInterruptSelf => false;

    public override bool CanEnterState => true;
    
    public override bool CanExitState
    {
        get
        {
            // There are several different ways of accessing the state change details:
            // CharacterState nextState = StateChange<CharacterState>.NextState;
            // CharacterState nextState = this.GetNextState();
            BossState nextState = boss.StateMachine.NextState;
            if (nextState == this)
                return CanInterruptSelf;
            if (Priority == BossStatePriority.Low)
                return true;
            
            return nextState.Priority > Priority;
        }
    }
    
    public event Action OnStateEnd;
    
    protected void EndState()
    {
        OnStateEnd?.Invoke();
    }
    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        gameObject.GetComponentInParentOrChildren(ref boss);
    }
#endif
}
