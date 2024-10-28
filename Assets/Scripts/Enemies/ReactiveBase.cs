using UnityEngine;

public class ReactiveBase : EnemyBase
{
    protected enum EnemyState
    {
        Scanning,
        Moving
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

            case EnemyState.Moving:
                
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
