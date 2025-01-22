using System;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using System.Linq;
using Random = UnityEngine.Random;

public class DraculaBrain : MonoBehaviour 
{
    [SerializeField] private BossBase boss;
    [SerializeField] private int baseAttackQueueSize = 3;
    [SerializeField] private int attackQueueSizeIncreaseFactor = 1;
    
    private BossState[] _states;
    private List<BossState> _attackQueue = new List<BossState>();
    private BossState _currentState;
    private int _currentPhase = 1;
    private bool _lastInQueue;
    private bool _isFighting;
    
    private void Awake()
    {
        _states = GetComponentsInChildren<DraculaState>();

        foreach (var state in _states)
        {
            state.OnStateEnd += CurrentStateEnded;
        }
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

    private BossState[] GetStatesByPhase(int phase)
    {
        return _states.Where(state => state.StatePhase == phase).ToArray();
    }

    private void FillAttackQueue()
    {
        if (_attackQueue.Count > 0)
        {
            return;
        }
        
        var amount = baseAttackQueueSize + attackQueueSizeIncreaseFactor * (boss.MaxHealth - boss.CurrentHealth);
        var states = GetStatesByPhase(_currentPhase);
        for (var i = 0; i < amount; i++)
        {
            _attackQueue.Add(states[Random.Range(0, states.Length)]);
            print(_attackQueue[i]);
        }
        
        // Add PingPongState to the end of the queue
        _attackQueue.Add(GetStatesByPhase(_currentPhase).First(state => state.GetType() == typeof(PingPongState)));
    }
    
    private void MoveToNextState()
    {
        _currentState = _attackQueue[0];
        boss.StateMachine.ForceSetState(_currentState);
        _attackQueue.RemoveAt(0);
        _lastInQueue = _attackQueue.Count == 0;
    }
    
    private void CurrentStateEnded()
    {
        if (!_isFighting) return;
        
        if (_lastInQueue)
        {
            FillAttackQueue();
        }
        else
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
