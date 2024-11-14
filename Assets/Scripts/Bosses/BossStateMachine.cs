using UnityEngine;

public class BossStateMachine
{
    private BossState currentState;
    
    public void SetState(BossState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        
        currentState = newState;
        currentState.Enter();
    }
    
    private void Update()
    {
        currentState.Update();
    }
}
