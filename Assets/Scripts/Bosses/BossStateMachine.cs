using UnityEngine;

public class BossStateMachine
{
    private BossState _currentState;
    
    public void SetState(BossState state)
    {
        if (_currentState != null)
        {
            _currentState.Exit();
        }
        
        _currentState = state;
        _currentState.Enter();
    }
    
    public void Update()
    {
        if (_currentState != null)
        {
            _currentState.Update();
            Debug.Log(_currentState.GetType().Name);
        }
    }
}
