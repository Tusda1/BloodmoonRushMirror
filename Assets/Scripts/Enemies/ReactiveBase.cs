using UnityEngine;

public class ReactiveBase : EnemyBase
{
    [SerializeField] protected float detectionDistance = 20f;
    
    protected Transform targetObject;
    
    protected enum EnemyState
    {
        Scanning,
        Action
    }
    
    protected EnemyState currentState = EnemyState.Scanning;
    
    protected new void Update()
    {
        base.Update();

        if (IsDead) { return; }

        if (IsStunned) { return; }

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
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionDistance, LayerMask.GetMask("Player"));
        if (player != null && player.CompareTag("Player"))
        {
            currentState = EnemyState.Action;
            targetObject = player.transform;
        }
    }
    
    protected virtual void Action()
    {
        
    }
}
