using System;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using System.Linq;
using Random = UnityEngine.Random;

public enum DraculaStateCategory
{
    Phase1,
    Phase2,
    General
}

public class DraculaBrain : MonoBehaviour 
{
    [SerializeField] private BossBase boss;
    [SerializeField] private int baseAttackQueueSize = 3;
    [SerializeField] private int attackQueueSizeIncreaseFactor = 1;
    
    private DraculaState[] _states;
    private List<DraculaState> _attackQueue = new List<DraculaState>();
    private DraculaState _currentState;
    private int _currentPhase = 1;
    private bool _lastInQueue;
    private bool _isFighting;
    
    private void Awake()
    {
        _states = GetComponentsInChildren<DraculaState>();
    }

    private void Start()
    {
        boss.OnBattleStart += StartFighting;
    }
    
    private void StartFighting()
    {
        _isFighting = true;
        FillAttackQueue();
        MoveToNextState();
    }
    
    private void PauseFight()
    {
        _isFighting = false;
    }
    
    private void ResumeFight()
    {
        _isFighting = true;
    }
    
    private void EndFight()
    {
        _isFighting = false;
    }

    private DraculaState[] GetStatesByCategory(DraculaStateCategory category)
    {
        return _states.Where(state => state.Category == category).ToArray();
    }

    private void FillAttackQueue()
    {
        if (_attackQueue.Count > 0)
        {
            return;
        }
        
        var amount = baseAttackQueueSize + attackQueueSizeIncreaseFactor * (boss.MaxHealth - boss.CurrentHealth);
        var states = GetStatesByCategory(_currentPhase == 1 ? DraculaStateCategory.Phase1 : DraculaStateCategory.Phase2);
        for (var i = 0; i < amount; i++)
        {
            _attackQueue.Add(states[Random.Range(0, states.Length)]);
        }
        
        // Add PingPongState to the end of the queue
        _attackQueue.Add(GetStatesByCategory(DraculaStateCategory.General).First(state => state.GetType() == typeof(PingPongState)));
    }
    
    private void MoveToNextState()
    {
        _currentState = _attackQueue[0];
        boss.StateMachine.ForceSetState(_currentState);
        _attackQueue.RemoveAt(0);
        _lastInQueue = _attackQueue.Count == 0;
    }

    private void Update()
    {
        if (!_isFighting) return;
        
        if (_currentState.StateFinished && _lastInQueue)
        {
            FillAttackQueue();
        }
        else if(_currentState.StateFinished)
        {
            MoveToNextState();
        }
    }


#if UNITY_EDITOR
    protected void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref boss);
    }
#endif
}
