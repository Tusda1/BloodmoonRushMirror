using System;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using System.Linq;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

public class DraculaBrain : MonoBehaviour 
{
    [SerializeField] private BossBase boss;
    [SerializeField] private int[] baseAttackQueueSize;
    [SerializeField] private int[] attackQueueSizeIncreaseFactor;
    [SerializeField] private DiscoBall discoBall;
    
    private BossState[] _states;
    private List<BossState> _attackQueue = new();
    private BossState _currentState;
    private bool _lastInQueue;
    private bool _isFighting;
    private PingPongState[] _pingPongState;
    private bool _repeating;
    
    private IObjectResolver _objectResolver;
    private IAudioManager _audioManager;
    
    [Inject]
    public void Construct(LifetimeScope currentScope) {
        _objectResolver = currentScope.Container;
        _audioManager = _objectResolver.Resolve<IAudioManager>();
    }
    
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
        _states = _states.Where(state => state.GetType() != typeof(DraculaDeath)).ToArray();
        _states = _states.Where(state => state.GetType() != typeof(PreFightStateDracula)).ToArray();
        _states = _states.Where(state => state.GetType() != typeof(TransitionState)).ToArray();
        
        var discoBallState = GetComponent<DiscoBallActivation>();
        discoBallState.discoBall = discoBall;
        
        boss.OnBattleStart += StartFighting;
        boss.OnDeath += Death;
        boss.OnBattlePause += PauseFight;
        boss.OnBattleResume += ResumeFight;
        boss.OnReset += ResetFight;
        boss.DisableGravity();
        
        // Set the initial state to PreFightState
        var preFightState = GetComponent<PreFightStateDracula>();
        boss.StateMachine.ForceSetState(preFightState);
    }
    
    private void StartFighting()
    {
        _isFighting = true;
        FillAttackQueue();
        MoveToNextState();
        _audioManager.PlayMusic("Dracula_FightTheme");
    }

    private void Death()
    {
        if (boss.CurrentPhase == 0)
        {
            boss.StateMachine.ForceSetState(GetComponent<TransitionState>());
        }
        else
        {
            EndFight();
        }
    }
    
    private void PauseFight()
    {
        _isFighting = false;
        _audioManager.StopMusic("Dracula_FightTheme");
        boss.canTakeDamage = false;
        boss.canDoDamage = false;
    }
    
    private void ResumeFight()
    {
        boss.MoveToNextPhase();
        _isFighting = true;
        FillAttackQueue();
        MoveToNextState();
        _audioManager.PlayMusic("Dracula_FightTheme");
        boss.canDoDamage = true;
    }
    
    private void ResetFight()
    {
        _isFighting = false;
        boss.DisableGravity();
        boss.AnimancerComponent.States.Current?.Destroy();
        boss.transform.localScale = new Vector3(1, 1, 1);
        boss.StateMachine.ForceSetState(GetComponent<PreFightStateDracula>());
        discoBall.ResetDiscoBall();
        boss.canTakeDamage = false;
        boss.canDoDamage = true;
        _audioManager.StopMusic("Dracula_FightTheme");
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
            _repeating = false;
        }
        
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
    
    private void EndFight()
    {
        _isFighting = false;
        var deathState = GetComponent<DraculaDeath>();
        boss.StateMachine.ForceSetState(deathState);
        _audioManager.StopMusic("Dracula_FightTheme");
    }


#if UNITY_EDITOR
    protected void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref boss);
    }
#endif
}
