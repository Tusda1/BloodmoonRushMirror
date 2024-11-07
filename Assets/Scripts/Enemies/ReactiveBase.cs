using UnityEngine;

public class ReactiveBase : EnemyBase
{
    protected enum EnemyState
    {
        Scanning,
        Action
    }
    
    protected EnemyState currentState = EnemyState.Scanning;
    
    protected new void Update()
    {
        base.Update();

        if (isDead) { return; }

        if (isStunned) { return; }

        switch (currentState)
        {
            case EnemyState.Scanning:
                Scan();
                break;

            case EnemyState.Action:
                
                Action();
                break;
        }
    }
    
    protected virtual void Scan()
    {
        
    }
    
    protected virtual void Action()
    {
        
    }
}
