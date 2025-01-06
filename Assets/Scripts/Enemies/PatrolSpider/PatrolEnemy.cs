using System;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolEnemy : EnemyBase
{
    [SerializeField] private SmoothDampScript smoothDampScript = new();
    [SerializeField] private float ledgeDistance = 1f;
    [SerializeField] private float attackRange = 2f;
    
    private PatrolAnimation _patrolAnimation;
    private Vector2 _visualVelocity;

    protected override void Start()
    {
        base.Start();
        
        _patrolAnimation = EnemyAnimationBase as PatrolAnimation;
    }
    
    private new void Update()
    {
        base.Update();
        
        if (IsDead || IsStunned || !isGrounded || _patrolAnimation.isAttacking)
        {
            smoothDampScript.SetStartVelocity(velocity);
            return;
        }

        Patrol();
    }
    
    private void Patrol()
    {
        // Smoothly move towards the target
        smoothDampScript.SmoothDampFloat(FacingDirection, moveSpeed, ref targetVelocity);
        _visualVelocity = targetVelocity;
        
        // Check if there is a wall in front of the enemy
        RaycastHit2D hit = Physics2D.Raycast(GetBottomPosition(), FacingDirection, ledgeDistance, EnemyAndGroundLayerMask);
        if (hit.collider != null)
        {
            // Change direction
            FacingDirection = -FacingDirection;
        }
        
        // Check if there is a ledge in front of the enemy
        hit = Physics2D.Raycast(GetBottomPosition() + FacingDirection * ledgeDistance, Vector2.down, 1f, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            // Change direction
            FacingDirection = -FacingDirection;
        }
    }

    private void PatrolAttack()
    {
        if (Vector2.Distance(Player.transform.position, transform.position) > attackRange) return;
        
        base.Attack();
    }

    protected override void Attack()
    {
        _patrolAnimation.Attack(PatrolAttack);
    }

    protected override void SetVisualFacingDirection()
    {
        VisualFacingDirection = _visualVelocity.x > 0 ? Vector2.right : Vector2.left;
    }
}
