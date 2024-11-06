using Pathfinding;
using UnityEngine;

public class Skeleton : ReactiveBase
{
    [SerializeField] private float detectionDistance = 10f;
    [SerializeField] private float keepAgroDistance = 2f;

    private Transform _agroOject;
    
    protected override void Scan()
    {
        //Send a ray to the right
        RaycastHit2D hit = Physics2D.Raycast(GetBottomPosition() + Vector2.up, facingDirection, detectionDistance, LayerMask.GetMask("Player"));
        if (hit.collider != null)
        {
            currentState = EnemyState.Moving;
            _agroOject = hit.transform;
        }
        
        //Send a ray to the left
        hit = Physics2D.Raycast(GetBottomPosition() + Vector2.up, -facingDirection, detectionDistance, LayerMask.GetMask("Player"));
        if (hit.collider != null)
        {
            currentState = EnemyState.Moving;
            _agroOject = hit.transform;
        }
    }

    protected override void Action()
    {
        // Check if the target is still in range
        if (Vector2.Distance(_agroOject.position, transform.position) > keepAgroDistance)
        {
            currentState = EnemyState.Scanning;
            return;
        }
        
        // Check if the target is to the left or right
        facingDirection = (_agroOject.position.x > transform.position.x) ? Vector2.right : Vector2.left;
        
        // Move towards the target
        targetVelocity = (facingDirection * moveSpeed);
    }
}
