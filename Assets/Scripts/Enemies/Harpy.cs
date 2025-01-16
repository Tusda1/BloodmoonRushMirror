using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Seeker))]
public class Harpy : ReactiveBase
{
    [SerializeField] private float nextWaypointDistance;
    [SerializeField] private float hoverHeight;
    [SerializeField] private float xDistanceFromPlayer;
    [SerializeField] private float attackDistance;
    [SerializeField] private float yAttackPositionOffset;
    [SerializeField] private float pathUpdateInterval = 1f;
    [SerializeField] private SmoothDampScript smoothDampScript = new();
    
    private float _repeatTimer;
    private Vector2 _targetPosition;
    private Path _path;
    private int _currentWaypoint;
    private Seeker _seeker;
    private bool _swooping;
    
    protected override void Start()
    {
        base.Start();
        _seeker = GetComponent<Seeker>();
        GravityModifier = 0;
    }
    
    protected override void Scan()
    {
        var playerObject = Physics2D.OverlapCircle(transform.position, detectionDistance, LayerMask.GetMask("Player"));
        if (playerObject.CompareTag("Player"))
        {
            targetObject = playerObject.transform;
            _targetPosition = targetObject.GetComponent<Collider2D>().bounds.center;
            currentState = EnemyState.Action;
            StartPath();
        }
    }
    
    protected override void Action()
    {
        if (!_swooping)
        { 
            FollowPlayer();
        }
        else
        {
            Swoop();
        }
    }
    
    private void FollowPlayer()
    {
        if (_path == null) return;
        print(_currentWaypoint);
        print(_path.vectorPath.Count);
        if (_currentWaypoint >= _path.vectorPath.Count - 1)
        {
            if  (Random.value < 0.1f)
            {
                _swooping = true;
            }
            else
            {
                StartPath();
            }
            
            _repeatTimer = pathUpdateInterval;
            return;
        }

        if (_repeatTimer > 0)
        {
            _repeatTimer -= Time.deltaTime;
        }
        else
        {
            _repeatTimer = pathUpdateInterval;
            StartPath();
        }
        
        Vector2 direction = (_path.vectorPath[_currentWaypoint] - transform.position).normalized;
        Vector2 force = direction * moveSpeed;
        
        smoothDampScript.SmoothDampVector2(force, ref targetVelocity);
        
        float distance = Vector2.Distance(transform.position, _path.vectorPath[_currentWaypoint]);

        if (distance <= nextWaypointDistance && _currentWaypoint != _path.vectorPath.Count - 1)
        {
            _currentWaypoint++;
        }
    }
    
    private void Swoop()
    {
        // Swoop towards the player
        Vector2 direction = ((Vector3)(_targetPosition + Vector2.up * yAttackPositionOffset) - transform.position).normalized;
        Vector2 force = direction * moveSpeed;
        
        smoothDampScript.SmoothDampVector2(force, ref targetVelocity);
        
        if (Vector2.Distance(transform.position, _targetPosition) < attackDistance)
        {
            _swooping = false;
            StartPath();
        }
    }
    
    private void StartPath()
    {
        if (!_seeker.IsDone()) return;
        
        _seeker.StartPath(transform.position, FindHoverPoint(), OnPathComplete);
    }

    private Vector2 FindHoverPoint()
    {
        // Choose a random point above the player using hoverHeight
        var position = _targetPosition;
        var targetvector = Vector2.zero;
        
        while (true)
        {
            var randomX = Random.Range(-xDistanceFromPlayer, xDistanceFromPlayer);
            targetvector = new Vector2(position.x + randomX, position.y + hoverHeight);

            // Make sure the point is not inside ground
            if (Physics2D.OverlapPoint(targetvector, LayerMask.GetMask("Ground")) == null)
            {
                return targetvector;
            }
        }
    }
    
    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
            _currentWaypoint = 0;
        }
    }
}
