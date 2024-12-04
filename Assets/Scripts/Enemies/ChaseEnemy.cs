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
    private float _repeatTimer;
    
    private Path _path;
    private int _currentWaypoint = 0;
    private bool _reachedEndOfPath;
    private Seeker _seeker;
    private bool _isAttacking;

    protected override void Start()
    {
        base.Start();

        _seeker = GetComponent<Seeker>();
    }

    protected override void Action()
    {
        if (targetObject == null)
        {
            currentState = EnemyState.Scanning;
            return;
        }

        if (_isAttacking)
        {
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

        if (_currentWaypoint >= _path.vectorPath.Count)
        {
            return;
        }

        // Determine the direction to the next waypoint
        Vector2 direction = ((Vector2)_path.vectorPath[_currentWaypoint] - rb2d.position).normalized;

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

        // Check if the enemy has reached the next waypoint
        if (Vector2.Distance(rb2d.position, _path.vectorPath[_currentWaypoint]) < nextWaypointDistance)
        {
            _currentWaypoint++;
        }
        
        // Check if enemy is close enough to attack
        if (Vector2.Distance(rb2d.position, targetObject.transform.position) < attackDistance && !_isAttacking)
        {
            StartCoroutine(Attack());
        }
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
            // Modify the path to account for the radius
            for (int i = 0; i < p.vectorPath.Count; i++)
            {
                Vector3 direction = (p.vectorPath[i] - (Vector3)rb2d.position).normalized;
                p.vectorPath[i] += direction * radius;
            }

            _path = p;
            _currentWaypoint = 0;
        }
    }
    
    private IEnumerator Attack()
    {
        _isAttacking = true;
        
        // Attack logic here
        yield return null;
        
        _isAttacking = false;
    }
}