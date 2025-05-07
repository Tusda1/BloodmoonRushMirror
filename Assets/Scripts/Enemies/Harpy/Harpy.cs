using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Seeker))]
public class Harpy : ReactiveBase
{
    [SerializeField] private string attackSoundID;
    [SerializeField] private float nextWaypointDistance;
    [SerializeField] private float hoverHeight;
    [SerializeField] private float xDistanceFromPlayer;
    [SerializeField] private float attackDistance;
    [SerializeField] private float swoopSpeed = 10;
    [SerializeField] private float maxSwoopDuration = 2f;
    [SerializeField] private float swoopChance = 0.1f;
    [SerializeField] private float yAttackPositionOffset = 2f;
    [SerializeField] private float pathUpdateInterval = 1f;
    [SerializeField] private SmoothDampScript smoothDampScript = new();

    private float _actualHoverHeight;
    private float _repeatTimer;
    private float _swoopTimer;
    private Path _path;
    private int _currentWaypoint;
    private Seeker _seeker;
    private bool _swooping;
    private Vector2 force;
    
    private HarpyAnimations _animations;

    private new void Awake()
    {
        base.Awake();
        _animations = GetComponent<HarpyAnimations>();
        _seeker = GetComponent<Seeker>();
        GravityModifier = 0;
    }
    
    protected override void Scan()
    {
        var playerObject = Physics2D.OverlapCircle(transform.position, detectionDistance, LayerMask.GetMask("Player"));
        if (playerObject && playerObject.CompareTag("Player"))
        {
            targetObject = playerObject.transform;
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
        
        facingDirection = force.x > 0 ? Vector2.right : Vector2.left;
    }
    
    private void FollowPlayer()
    {
        if (_path == null) return;
       
        if (_currentWaypoint >= _path.vectorPath.Count - 1)
        {
            if  (Random.value < swoopChance)
            {
                StartSwoop();
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
        
        Vector2 direction = ((Vector2)(_path.vectorPath[_currentWaypoint] - transform.position)).normalized;
        force = direction * moveSpeed;
        
        smoothDampScript.SmoothDampVector2(force, ref targetVelocity);
        
        float distance = Vector2.Distance(transform.position, _path.vectorPath[_currentWaypoint]);

        if (distance <= nextWaypointDistance && _currentWaypoint != _path.vectorPath.Count - 1)
        {
            _currentWaypoint++;
        }
    }
    
    private void Swoop()
    {
        if (_swoopTimer > maxSwoopDuration)
        {
            StartFollow();
        }
        else
        {
            _swoopTimer += Time.deltaTime;
        }
        
        // Swoop towards the player
        Vector2 direction = ((Vector2)(targetObject.position + Vector3.up * yAttackPositionOffset - transform.position)).normalized;
        force = direction * swoopSpeed;
        
        smoothDampScript.SmoothDampVector2(force, ref targetVelocity);
        
        if (Vector2.Distance(transform.position, targetObject.position) < attackDistance)
        {
            StartFollow();
        }
    }

    private void StartFollow()
    {
        _animations.PlayDefaultAnimation();
        _swooping = false;
        _swoopTimer = 0;
        StartPath();
    }

    private void StartSwoop()
    {
        _swooping = true;
        AudioManager.PlayOneShotAttached(attackSoundID, gameObject);
        _animations.PlaySwoopAnimation();
    }
    
    private void StartPath()
    {
        if (!_seeker.IsDone()) return;
        
        _seeker.StartPath(transform.position, FindHoverPoint(), OnPathComplete);
    }

    private Vector2 FindHoverPoint()
    {
        // Choose a random point above the player using hoverHeight
        var position = targetObject.position;
        var targetvector = Vector2.zero;
        _actualHoverHeight = hoverHeight;
        int attempts = 0;
        
        while (true)
        {
            attempts++;
            var randomX = Random.Range(-xDistanceFromPlayer, xDistanceFromPlayer);
            targetvector = new Vector2(position.x + randomX, position.y + _actualHoverHeight);

            // Make sure the point is not inside ground
            if (Physics2D.OverlapPoint(targetvector, LayerMask.GetMask("Ground")) == null)
            {
                return targetvector;
            }

            _actualHoverHeight -= 0.5f;
            
            if (attempts > 10)
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
