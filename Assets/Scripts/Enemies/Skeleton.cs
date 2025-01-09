using Pathfinding;
using UnityEngine;

public class Skeleton : ReactiveBase
{
    [SerializeField] private float keepAgroDistance = 2f;
    [SerializeField] private SmoothDampScript smoothDampScript = new();
    
    protected override void Scan()
    {
        //Send a ray to the right
        RaycastHit2D hit = Physics2D.Raycast(GetBottomPosition() + Vector2.up, FacingDirection, detectionDistance, LayerMask.GetMask("Player"));
        if (hit.collider != null)
        {
            currentState = EnemyState.Action;
            targetObject = hit.transform;
        }
        
        //Send a ray to the left
        hit = Physics2D.Raycast(GetBottomPosition() + Vector2.up, -FacingDirection, detectionDistance, LayerMask.GetMask("Player"));
        if (hit.collider != null)
        {
            currentState = EnemyState.Action;
            targetObject = hit.transform;
        }
    }

    protected override void Action()
    {
        // Check if the target is still in range
        if (Vector2.Distance(targetObject.position, transform.position) > keepAgroDistance)
        {
            currentState = EnemyState.Scanning;
            return;
        }
        
        // Check if the target is to the left or right
        FacingDirection = (targetObject.position.x > transform.position.x) ? Vector2.right : Vector2.left;
        
        // Smoothly move towards the target
        smoothDampScript.SmoothDampFloat(FacingDirection, moveSpeed, ref targetVelocity);
    }
}
