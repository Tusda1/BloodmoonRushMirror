using System;
using UnityEngine;
using Animancer;

public class DrucillaBrain : MonoBehaviour 
{
    [SerializeField] private BossBase boss;
    
    private BossState[] _states;
    
    private void Awake()
    {
        boss.OnBattleStart += StartFighting;
        boss.OnBattleEnd += EndFighting;
        
        _states = GetComponentsInChildren<BossState>();

        foreach (var state in _states)
        {
            state.OnStateEnd += MoveToNextState;
        }
    }
    
    private void StartFighting()
    {
        MoveToNextState();
    }
    
    private void EndFighting()
    {
        
    }
    
    private void MoveToNextState()
    {
        // pick a random state from the list of states
        var nextState = _states[UnityEngine.Random.Range(0, _states.Length)];
        boss.StateMachine.ForceSetState(nextState);
    }
    
#if UNITY_EDITOR
    protected void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref boss);
    }
#endif
}
