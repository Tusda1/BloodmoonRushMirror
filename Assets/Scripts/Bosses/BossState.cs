using Animancer;
using Animancer.FSM;
using UnityEngine;
using System;

public enum BossStatePriority
{
    Low,
    Medium,
    High
}

public class BossState : StateBehaviour
{
    [SerializeField] private BossBase _boss;
    protected BossBase Boss => _boss;
    
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
            BossState nextState = _boss.StateMachine.NextState;
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

        gameObject.GetComponentInParentOrChildren(ref _boss);
    }
#endif
}
