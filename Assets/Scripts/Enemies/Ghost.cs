using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class Ghost : ReactiveBase
{
    [SerializeField] private float detectionRadius;
    [SerializeField] private float nextWaypointDistance;
    [SerializeField] private SmoothDampScript smoothDampScript = new();
    
    private Transform player;
    private Vector2 targetPosition;
    private Path path;
    private int currentWaypoint;
    private Seeker seeker;
    
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
            currentState = EnemyState.Action;
            player = playerObject.transform;
            InvokeRepeating(nameof(UpdatePath), 0, 0.5f);
        }
    }
    
    protected override void Action()
    {
        AttackPlayer();
    }
    
    private void AttackPlayer()
    {
        if (path == null) return;
        
        if (currentWaypoint >= path.vectorPath.Count)
        {
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

    private void UpdatePath()
    {
        if (seeker.IsDone())
        {
            StartPath();
        }
    }
    
    private void StartPath()
    {
        seeker.StartPath(transform.position, player.position, OnPathComplete);
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
