using UnityEngine;

public class ReactiveBase : EnemyBase
{
    [SerializeField] protected float detectionDistance = 20f;
    [SerializeField] private string detectionSoundID;
    
    protected Transform targetObject;
    
    protected enum EnemyState
    {
        Scanning,
        Action
    }
    
    protected EnemyState currentState = EnemyState.Scanning;
    
    protected override void ComputeVelocity()
    {
        if (isDead)
        {
            currentState = EnemyState.Scanning;
            return;
        }

        if (isStunned || isKnockedUp) { return; }

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
        if (player && player.CompareTag("Player"))
        {
            currentState = EnemyState.Action;
            targetObject = player.transform;
            if (!string.IsNullOrEmpty(detectionSoundID)) AudioManager.PlayOneShotAttached(detectionSoundID, gameObject);
        }
    }
    
    protected virtual void Action()
    {
        
    }

    public override void Respawn()
    {
        base.Respawn();
        currentState = EnemyState.Scanning;
    }
}
