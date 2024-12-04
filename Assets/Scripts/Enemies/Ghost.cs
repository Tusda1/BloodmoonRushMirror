using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class Ghost : ReactiveBase
{
    [SerializeField] private float detectionRadius;
    [SerializeField] private float attackDistance;
    [SerializeField] private float maxWithdrawDistance = 5f;
    [SerializeField] private float minWithdrawDistance = 3f;
    [SerializeField] private SmoothDampScript smoothDampScript = new();
    
    private Transform _player;
    private bool _withdrawing;
    private Vector2 _withdrawPosition;
    
    protected override void Start()
    {
        base.Start();
        GravityModifier = 0;
    }
    
    protected override void Scan()
    {
        var playerObject = Physics2D.OverlapCircle(transform.position, detectionRadius, LayerMask.GetMask("Player"));
        if (playerObject)
        {
            currentState = EnemyState.Action;
            _player = playerObject.transform;
        }
    }
    
    protected override void Action()
    {
        if (!_withdrawing)
        {
            AttackPlayer();
        }
        else
        {
            Withdraw();
        }
        
    }
    
    private void AttackPlayer()
    {
        if (_player == null) return;

        var position = transform.position;
        Vector2 direction = (_player.position - position).normalized * moveSpeed;
        smoothDampScript.SmoothDampVector2(direction, ref targetVelocity);
        
        if (Vector2.Distance(position, _player.position) < attackDistance)
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

            _withdrawPosition = (Vector2)_player.position + randomPosition;
        }

        
        Vector2 direction = (_withdrawPosition - (Vector2)transform.position).normalized * moveSpeed;
        smoothDampScript.SmoothDampVector2(direction, ref targetVelocity);
        
        if (Vector2.Distance(transform.position, _withdrawPosition) < 0.5f)
        {
            _withdrawing = false;
            _withdrawPosition = Vector2.zero;
        }
    }
}
