using System;
using System.Collections;
using UnityEngine;
using Pathfinding;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Seeker))]
public class LivingArmor : ReactiveBase
{
    [SerializeField] private float horizontalJumpSpeed = 8f;
    [SerializeField] private float staggerDuration;
    [SerializeField] private Collider2D highAttackCollider;
    [SerializeField] private Collider2D lowAttackCollider;
    [SerializeField] private Collider2D overheadAttackCollider;
    [SerializeField] private float nextWaypointDistance = 0.5f;
    [SerializeField] private float repeatRate = 0.5f;
    [SerializeField] private float radius = 0.5f; // Define the radius property
    [SerializeField] private float attackDistance = 1f;
    [SerializeField] private float minimumVerticalJumpAngle = 15f;
    [SerializeField] private float maxVerticalJumpAngle = 60f;
    [SerializeField] private float minimumHorizontalJumpAngle = 75f;
    [SerializeField] private float maxHorizontalJumpAngle = 150f;
    
    private float _repeatTimer;
    
    private int _currentWaypoint = 0;
    private bool _reachedEndOfPath;
    private bool _isAttacking;
    private bool _isJumping;
    
    [SerializeField] private Shield shield = new();
    private Path _path;
    private Seeker _seeker;

    protected override void Start()
    {
        base.Start();

        _seeker = GetComponent<Seeker>();
    }
    
    private void StartPath()
    {
        if (targetObject == null) { return; }

        if (!_seeker.IsDone()) { return; }

        // Start the path without NNConstraint
        _seeker.StartPath(rb2d.position, targetObject.transform.position, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            /*// Modify the path to account for the radius
            for (int i = 0; i < p.vectorPath.Count; i++)
            {
                Vector3 direction = (p.vectorPath[i] - (Vector3)rb2d.position).normalized;
                p.vectorPath[i] += direction * radius;
            }*/

            _path = p;
            _currentWaypoint = 0;
        }
    }

    protected override void Action()
    {
        if (!targetObject)
        {
            currentState = EnemyState.Scanning;
            return;
        }

        if (_repeatTimer > 0)
        {
            _repeatTimer -= Time.deltaTime;
        }
        else
        {
            _repeatTimer = repeatRate;
            StartPath();
        }

        if (_path == null) { return; }
        
        // Check if the enemy has reached the next waypoint
        if (Vector2.Distance(rb2d.position, _path.vectorPath[_currentWaypoint]) < nextWaypointDistance && _currentWaypoint < _path.vectorPath.Count - 1)
        {
            _currentWaypoint++;
        }
        
        if (_isJumping) return;
        
        AttackCheck();
        
        Move();
    }

    private void Move()
    {
        if (_isAttacking) return;
        
        // Determine the direction to the next waypoint
        Vector2 direction = ((Vector2)_path.vectorPath[_currentWaypoint] - rb2d.position).normalized;
        var angle = Vector2.Angle(Vector2.up, direction);
        
        // overlappoint to check if the enemy is walking off a ledge
        Collider2D groundHit = Physics2D.OverlapPoint(GetBottomPosition() + direction * 0.5f, LayerMask.GetMask("Ground"));
        if (!groundHit)
        {
            Collider2D nextGroundHit = Physics2D.OverlapPoint((Vector2)_path.vectorPath[_currentWaypoint] + Vector2.down, LayerMask.GetMask("Ground"));
            if (!nextGroundHit && angle > minimumHorizontalJumpAngle && angle < maxHorizontalJumpAngle)
            {
                StartCoroutine(JumpCoroutine(CalculateHorizontalJump));
            }
        }
        
        if (isGrounded && angle > minimumVerticalJumpAngle && angle < maxVerticalJumpAngle)
        {
            StartCoroutine(JumpCoroutine(CalculateVerticalJump));
        }
        else if (angle < minimumVerticalJumpAngle)
        {
            targetVelocity = facingDirection * moveSpeed;
        }
        else
        {
            // Add a threshold to avoid jittering
            float directionThreshold = 0.1f;
            if (Mathf.Abs(direction.x) > directionThreshold)
            {
                facingDirection = direction.x > 0 ? Vector2.right : Vector2.left;
            }

            // Move towards the next waypoint if grounded and angle to next waypoint is less than 75 degrees
            if (isGrounded && !(Vector2.Angle(Vector2.up, direction) < 15))
            {
                targetVelocity = facingDirection * moveSpeed;
            }
        }
    }

    private IEnumerator JumpCoroutine(Func<Vector2> calculationMethod)
    {
        _isJumping = true;
        
        facingDirection = ((Vector2)_path.vectorPath[_currentWaypoint] - rb2d.position).x > 0 ? Vector2.right : Vector2.left;
        
        var jumpVelocity = calculationMethod();
        velocity.y = jumpVelocity.y;
        
        while (isGrounded)
        {
            targetVelocity.x = jumpVelocity.x;
            
            yield return null;
        }

        while (!isGrounded)
        {
            targetVelocity.x = jumpVelocity.x;
            
            yield return null;
        }
        
        _isJumping = false;
    }
    
    private Vector2 CalculateVerticalJump()
    {
        var height = ((Vector2)_path.vectorPath[_currentWaypoint] - rb2d.position).y;
        var length = ((Vector2)_path.vectorPath[_currentWaypoint] - rb2d.position).x;
        var velocityY = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y) * height);
        var jumpTime = velocityY / Mathf.Abs(Physics2D.gravity.y);
        var velocityX = length / jumpTime;
        return new Vector2(velocityX, velocityY);
    }

    private Vector2 CalculateHorizontalJump()
    {
        var height = ((Vector2)_path.vectorPath[_currentWaypoint] - rb2d.position).y;
        var length = ((Vector2)_path.vectorPath[_currentWaypoint] - rb2d.position).x;
        var speed = horizontalJumpSpeed;
        
        //Calculate if the you will overshoot the target with only gravity on Y axis
        while (true)
        {
            var timeToReachHeight = Mathf.Sqrt(2 * height) / Mathf.Abs(Physics2D.gravity.y);
            var overshoot = timeToReachHeight * horizontalJumpSpeed - length;

            if (speed < 1)
            {
                break;
            }
            if (overshoot > 0)
            {
                speed -= 0.1f;
            }
            else
            {
                break;
            }
        }

        var jumpTime = length / speed;
        var velocityY = (height - 0.5f * Mathf.Abs(Physics2D.gravity.y) * jumpTime * jumpTime) / jumpTime;
        return new Vector2(speed, velocityY);
    }
    
    private void AttackCheck()
    {
        // Use a overlap box to check if the player is within the bound of the overhead attack collider
        var bounds = overheadAttackCollider.bounds;
        Collider2D playerCollider = Physics2D.OverlapBox(bounds.center, bounds.size, 0, LayerMask.GetMask("Player"));
        if (playerCollider != null)
        {
            OverHeadAttack();
        }
        
        // Check if enemy is close enough to attack
        if (Vector2.Distance(rb2d.position, targetObject.transform.position) < attackDistance && !_isAttacking)
        {
            ExecuteAttack();
        }
    }

    private void ExecuteAttack()
    {
        // 50% chance to do high attack or low attack
        if (Random.value > 0.5f)
        {
            HighAttack();
        }
        else
        {
            LowAttack();
        }
    }

    private void HighAttack()
    {
        highAttackCollider.enabled = true;
        _isAttacking = true;
        _isAttacking = false;
    }

    private void LowAttack()
    {
        lowAttackCollider.enabled = true;
        _isAttacking = true;
        _isAttacking = false;
    }

    private void OverHeadAttack()
    {
        overheadAttackCollider.enabled = true;
        _isAttacking = true;
        _isAttacking = false;
    }

    protected override bool CanBeDamaged(DamageInstance damageInstance)
    {
        shield.ShieldBlock(base.Attack, player, damageInstance);
        
        if (damageInstance.which == DamageInstance.WhichAttack.CrouchAttack) StartCoroutine(Stagger());
        
        return damageInstance.which == DamageInstance.WhichAttack.CrouchAttack;
    }

    private IEnumerator Stagger()
    {
        isStunned = true;
        yield return new WaitForSeconds(staggerDuration);
        isStunned = false;
    }
}