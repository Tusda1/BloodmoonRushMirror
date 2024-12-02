using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Seeker))]
public class Harpy : ReactiveBase
{
    [SerializeField] private float detectionRadius;
    [SerializeField] private float nextWaypointDistance;
    [SerializeField] private float hoverHeight;
    [SerializeField] private float xDistanceFromPlayer;
    [SerializeField] private float attackDistance;
    [SerializeField] private SmoothDampScript smoothDampScript = new();
    
    
    private Transform player;
    private Vector2 targetPosition;
    private Path path;
    private int currentWaypoint;
    private Seeker seeker;
    private bool _swooping;
    
    protected override void Start()
    {
        base.Start();
        seeker = GetComponent<Seeker>();
        GravityModifier = 0;
    }
    
    protected override void Scan()
    {
        var playerObject = Physics2D.OverlapCircle(transform.position, detectionRadius, LayerMask.GetMask("Player"));
        if (playerObject)
        {
            player = playerObject.transform;
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
        if (path == null) return;
        
        if (currentWaypoint >= path.vectorPath.Count)
        {
            if  (Random.value < 0.1f)
            {
                _swooping = true;
            }
            else
            {
                StartPath();
            }
            
            return;
        }
        
        Vector2 direction = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        Vector2 force = direction * moveSpeed;
        
        smoothDampScript.SmoothDampVector2(force, ref targetVelocity);
        
        float distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);

        if (distance <= nextWaypointDistance && currentWaypoint != path.vectorPath.Count - 1)
        {
            currentWaypoint++;
        }
    }
    
    private void Swoop()
    {
        // Swoop towards the player
        Vector2 direction = (player.position - transform.position).normalized;
        Vector2 force = direction * moveSpeed;
        
        smoothDampScript.SmoothDampVector2(force, ref targetVelocity);
        
        if (Vector2.Distance(transform.position, player.position) < attackDistance)
        {
            _swooping = false;
            StartPath();
        }
    }
    
    private void StartPath()
    {
        seeker.StartPath(transform.position, FindHoverPoint(), OnPathComplete);
    }

    private Vector2 FindHoverPoint()
    {
        // Choose a random point above the player using hoverHeight
        var position = player.position;
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
            path = p;
            currentWaypoint = 0;
        }
    }
}
