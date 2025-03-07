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
        
        _patrolAnimation = enemyAnimationBase as PatrolAnimation;
    }
    
    private new void Update()
    {
        base.Update();
        
        if (isDead || isStunned || !isGrounded || _patrolAnimation.isAttacking)
        {
            smoothDampScript.SetStartVelocity(velocity);
            return;
        }

        Patrol();
    }
    
    private void Patrol()
    {
        // Smoothly move towards the target
        smoothDampScript.SmoothDampVector2(facingDirection * moveSpeed, ref targetVelocity);
        _visualVelocity = targetVelocity;
        
        // Check if there is a wall in front of the enemy
        RaycastHit2D hit = Physics2D.Raycast(GetBottomPosition(), facingDirection, ledgeDistance, enemyAndGroundLayerMask);
        if (hit.collider != null)
        {
            // Change direction
            facingDirection = -facingDirection;
        }
        
        // Check if there is a ledge in front of the enemy
        hit = Physics2D.Raycast(GetBottomPosition() + facingDirection * ledgeDistance, Vector2.down, 1f, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            // Change direction
            facingDirection = -facingDirection;
        }
    }

    private void PatrolAttack()
    {
        if (Vector2.Distance(player.transform.position, transform.position) > attackRange) return;
        
        base.Attack();
    }

    protected override void Attack()
    {
        if (_patrolAnimation.isAttacking) return;
        AudioManager.PlayOneShot("Enemy_Spider_Hit");
        _patrolAnimation.Attack(PatrolAttack);
    }

    protected override void SetVisualFacingDirection()
    {
        visualFacingDirection = _visualVelocity.x > 0 ? Vector2.right : Vector2.left;
    }

    protected override void PlayHitSoundEffect()
    {
        AudioManager.PlayOneShot("Enemy_Spider_Hit");
    }
}
