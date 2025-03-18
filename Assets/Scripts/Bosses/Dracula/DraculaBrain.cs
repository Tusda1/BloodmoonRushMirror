using System;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using System.Linq;
using Random = UnityEngine.Random;

public class DraculaBrain : MonoBehaviour 
{
    [SerializeField] private BossBase boss;
    [SerializeField] private int[] baseAttackQueueSize;
    [SerializeField] private int[] attackQueueSizeIncreaseFactor;
    
    private BossState[] _states;
    private List<BossState> _attackQueue = new();
    private BossState _currentState;
    private bool _lastInQueue;
    private bool _isFighting;
    private PingPongState[] _pingPongState;
    private bool _repeating;
    
    private void Awake()
    {
        boss.canTakeDamage = false;
        
        _states = GetComponentsInChildren<BossState>();

        foreach (var state in _states)
        {
            state.OnStateEnd += CurrentStateEnded;
        }
        
        //remove PingPongState from the list of states to prevent it from being added to the attack queue and set it as a separate variable
        _pingPongState = _states.Where(state => state.GetType() == typeof(PingPongState)).Cast<PingPongState>().ToArray();
        
        _states = _states.Where(state => state.GetType() != typeof(PingPongState)).ToArray();
        _states = _states.Where(state => state.GetType() != typeof(VulnerableState)).ToArray();
    }

    private void Start()
    {
        boss.OnBattleStart += StartFighting;
        boss.DisableGravity();
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
        if (boss.CurrentHealth <= 0) boss.MoveToNextPhase();
        
        var amount = baseAttackQueueSize[boss.CurrentPhase] + attackQueueSizeIncreaseFactor[boss.CurrentPhase] * (boss.MaxHealth[boss.CurrentPhase] - boss.CurrentHealth);
        var states = GetStatesByPhase(boss.CurrentPhase);
        while (_attackQueue.Count < amount)
        {
            var state = states[Random.Range(0, states.Length)];

            if (!_repeating && _attackQueue.Count > 0 && state == _attackQueue[^1])
            {
                _repeating = true;
                continue;
            }
            
            _attackQueue.Add(state);
            print(state);
            _repeating = false;
        }
        
        print(_pingPongState.Length);
        // Add the pingPongState that corresponds to the currentPhase to the end of the queue
        _attackQueue.Add(_pingPongState.First(state => state.StatePhase == boss.CurrentPhase));
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
            MoveToNextState();
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
