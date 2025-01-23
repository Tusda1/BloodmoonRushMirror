using System.Collections;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class ChaseEnemy : ReactiveBase
{
    [SerializeField] private float nextWaypointDistance = 0.5f;
    [SerializeField] private float repeatRate = 0.5f;
    [SerializeField] private float radius = 0.5f; // Define the radius property
    [SerializeField] private float attackDistance = 1f;
    [SerializeField] private float minimumJumpAngle = 15f;
    [SerializeField] private float maxJumpAngle = 60f;
    private float _repeatTimer;
    
    private int _currentWaypoint = 0;
    private bool _reachedEndOfPath;
    private bool _isAttacking;
    private bool _isJumping;
    
    private Path _path;
    private Seeker _seeker;
    private Shield _shield = new();

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

        if (_isAttacking || _isJumping)
        {
            return;
        }
        
        // Check if enemy is close enough to attack
        if (Vector2.Distance(rb2d.position, targetObject.transform.position) < attackDistance && !_isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
        
        // Check if the enemy has reached the next waypoint
        if (Vector2.Distance(rb2d.position, _path.vectorPath[_currentWaypoint]) < nextWaypointDistance && _currentWaypoint < _path.vectorPath.Count - 1)
        {
            _currentWaypoint++;
        }

        // Determine the direction to the next waypoint
        Vector2 direction = ((Vector2)_path.vectorPath[_currentWaypoint] - rb2d.position).normalized;
        var angle = Vector2.Angle(Vector2.up, direction);

        if (isGrounded && angle > minimumJumpAngle && angle < maxJumpAngle)
        {
            StartCoroutine(JumpCoroutine());
        }
        else if (angle < minimumJumpAngle)
        {
            targetVelocity = facingDirection * moveSpeed;
        }
        else
        {
            Move(direction);
        }
    }

    private void Move(Vector2 direction)
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

    private IEnumerator JumpCoroutine()
    {
        _isJumping = true;
        
        facingDirection = ((Vector2)_path.vectorPath[_currentWaypoint] - rb2d.position).x > 0 ? Vector2.right : Vector2.left;
        
        var jumpVelocity = CalculateJump();
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
    
    private Vector2 CalculateJump()
    {
        var height = ((Vector2)_path.vectorPath[_currentWaypoint] - rb2d.position).y;
        var velocityY = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y) * height);
        var jumpTime = velocityY / Mathf.Abs(Physics2D.gravity.y);
        var velocityX = ((Vector2)_path.vectorPath[_currentWaypoint] - rb2d.position).x / jumpTime;
        return new Vector2(velocityX, velocityY);
    }
    
    private IEnumerator AttackCoroutine()
    {
        _isAttacking = true;
        
        // Attack logic here
        yield return null;
        
        _isAttacking = false;
    }

    protected override bool CanBeDamaged(DamageInstance damageInstance)
    {
        _shield.ShieldBlock(base.Attack, player, damageInstance);
        
        return damageInstance.which == DamageInstance.WhichAttack.CrouchAttack;
    }
}