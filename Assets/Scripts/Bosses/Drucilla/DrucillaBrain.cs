using System;
using UnityEngine;
using Animancer;
using VContainer;
using VContainer.Unity;

public class DrucillaBrain : MonoBehaviour 
{
    [SerializeField] private BossBase boss;
    
    private bool _isFighting;
    
    private BossState[] _states;
    private IObjectResolver _objectResolver;
    private IAudioManager _audioManager;
    
    [Inject]
    public void Construct(LifetimeScope currentScope) {
        _objectResolver = currentScope.Container;
        _audioManager = _objectResolver.Resolve<IAudioManager>();
    }
    
    private void Awake()
    {
        boss.OnBattleStart += StartFighting;
        boss.OnDeath += EndFighting;
        
        _states = GetComponentsInChildren<BossState>();
        _states = Array.FindAll(_states, state => state is not DefeatState);

        foreach (var state in _states)
        {
            state.OnStateEnd += MoveToNextState;
        }
    }
    
    public void StartFighting()
    {
        // Play the boss music
        _audioManager.PlayMusic("Drucilla_FightTheme");
        _isFighting = true;
        MoveToNextState();
    }
    
    private void EndFighting()
    {
        // Stop the boss music
        _audioManager.StopMusic("Drucilla_FightTheme");
        _isFighting = false;
        var state = GetComponent<DefeatState>();
        boss.StateMachine.ForceSetState(state);
    }
    
    private void MoveToNextState()
    {
        if (!_isFighting) return;
        
        // pick a random state from the list of states
        var nextState = _states[UnityEngine.Random.Range(0, _states.Length)];
        boss.StateMachine.ForceSetState(nextState);
        print(boss.StateMachine.CurrentState);
    }
    
#if UNITY_EDITOR
    protected void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref boss);
    }
#endif
}
