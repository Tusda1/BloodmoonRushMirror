using UnityEngine;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(1)]
public class Ghost : ReactiveBase
{
    [SerializeField] private float attackDistance;
    [SerializeField] private float maxWithdrawDistance = 5f;
    [SerializeField] private float minWithdrawDistance = 3f;
    [SerializeField] private float minSlowDownDistance = 4f;
    [SerializeField] private float maxSlowDownDistance = 10f;
    [SerializeField] private float minSpeedPercentage = 0.8f;
    [SerializeField] private SmoothDampScript smoothDampScript = new();
    
    [HideInInspector] public bool sucked;
    [HideInInspector] public Vector2 suckVelocity;
    
    private bool _withdrawing;
    private Vector2 _withdrawPosition;
    private Collider2D _targetCollider;

    private GhostAnimations _ghostAnimations;
    
    protected override void Start()
    {
        base.Start();
        GravityModifier = 0;
        _ghostAnimations = enemyAnimationBase as GhostAnimations;
        _ghostAnimations?.PlayDefaultAnimation();
    }

    protected override void ComputeVelocity()
    {
        base.ComputeVelocity();
        
        if (sucked)
        {
            EscapeSuck();
        }
    }

    private void LateUpdate()
    {
        if (sucked)
        {
            facingDirection = targetVelocity.x > 0 ? Vector2.left : Vector2.right;
            sucked = false;
        }
        else
        {
            facingDirection = targetVelocity.x > 0 ? Vector2.right : Vector2.left;
        }
    }

    protected override void Action()
    {
        RepelGhosts();
        
        if (sucked) return;
        
        if (!_withdrawing)
        {
            AttackPlayer();
        }
        else
        {
            Withdraw();
        }
    }

    private void RepelGhosts()
    {
        // Use overlapcircle to see if there are any ghosts nearby
        var ghost = Physics2D.OverlapCircle(transform.position, 1f, LayerMask.GetMask("Enemy"));
        if (!ghost || !ghost.TryGetComponent(out Ghost ghostComponent)) return;
            
        // make the ghost move a little away from the other ghosts
        if (Vector2.Distance(ghostComponent.transform.position, transform.position) != 0)
        {
            var direction = (ghostComponent.transform.position - transform.position).normalized;
            targetVelocity += -(Vector2)direction;
        }
    }
    
    private void AttackPlayer()
    {
        if (!targetObject) return;

        if (!_targetCollider) _targetCollider = targetObject.GetComponent<Collider2D>();
        
        MoveToTarget(_targetCollider.bounds.center);
        
        if (Vector2.Distance(transform.position, _targetCollider.bounds.center) < attackDistance)
        {
            _withdrawing = true;
        }
    }
    
    private void Withdraw()
    {
        if (_withdrawPosition == Vector2.zero)
        {
            // Get random distance from the player
            Vector2 randomPosition = Vector2.zero;
            while (randomPosition.magnitude < minWithdrawDistance)
            {
                randomPosition = Random.insideUnitCircle * maxWithdrawDistance;
            }

            _withdrawPosition = (Vector2)_targetCollider.bounds.center + randomPosition;
        }
        
        MoveToTarget(_withdrawPosition);
        
        if (Vector2.Distance(transform.position, _withdrawPosition) < 0.5f)
        {
            _withdrawing = false;
            _withdrawPosition = Vector2.zero;
        }
    }

    private void MoveToTarget(Vector2 targetPosition)
    {
        var realMoveSpeed = moveSpeed;
        if (Vector2.Distance(targetPosition, transform.position) < minSlowDownDistance)
        {
            realMoveSpeed *= minSpeedPercentage;
        }
        else if (Vector2.Distance(targetPosition, transform.position) > minSlowDownDistance)
        {
            realMoveSpeed = Mathf.Lerp(moveSpeed * minSpeedPercentage, moveSpeed, Mathf.Clamp(Vector2.Distance(targetPosition, transform.position) - minSlowDownDistance / maxSlowDownDistance, 0, 1));
        }
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized * realMoveSpeed;
        var tempVelocity = Vector2.zero;
        smoothDampScript.SmoothDampVector2(direction, ref tempVelocity);
        targetVelocity += tempVelocity;
    }
    
    private void EscapeSuck()
    {
        _ghostAnimations.PlayGettingSuckedAnimation();
        // create an escape vector pointing away from the altar
        var direction = (-suckVelocity).normalized * moveSpeed;
        var tempVelocity = Vector2.zero;
        smoothDampScript.SmoothDampVector2(direction, ref tempVelocity);
        //targetVelocity += tempVelocity;
        targetVelocity += suckVelocity;
    }

    public void Execute()
    {
        transform.position = startPosition;
        currentState = EnemyState.Scanning;
        smoothDampScript.SetStartVelocity(Vector2.zero);
        ResetMovement(Axis.XAxis);
        ResetMovement(Axis.YAxis);
        _ghostAnimations.PlayDefaultAnimation();
    }
}
