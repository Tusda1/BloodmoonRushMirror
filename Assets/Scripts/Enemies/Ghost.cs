using UnityEngine;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(1)]
public class Ghost : ReactiveBase
{
    [SerializeField] private float attackDistance;
    [SerializeField] private float maxWithdrawDistance = 5f;
    [SerializeField] private float minWithdrawDistance = 3f;
    [SerializeField] private SmoothDampScript smoothDampScript = new();
    
    [HideInInspector] public bool sucked;
    [HideInInspector] public Vector2 suckVelocity;
    
    private bool _withdrawing;
    private Vector2 _withdrawPosition;
    
    protected override void Start()
    {
        base.Start();
        GravityModifier = 0;
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
            sucked = false;
        }
    }

    protected override void Action()
    {
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
    
    private void AttackPlayer()
    {
        if (targetObject == null) return;

        var position = transform.position;
        Vector2 direction = (targetObject.position - position).normalized * moveSpeed;
        smoothDampScript.SmoothDampVector2(direction, ref targetVelocity);
        
        if (Vector2.Distance(position, targetObject.position + Vector3.up) < attackDistance)
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

            _withdrawPosition = (Vector2)targetObject.position + randomPosition;
        }

        
        Vector2 direction = (_withdrawPosition - (Vector2)transform.position).normalized * moveSpeed;
        smoothDampScript.SmoothDampVector2(direction, ref targetVelocity);
        
        if (Vector2.Distance(transform.position, _withdrawPosition) < 0.5f)
        {
            _withdrawing = false;
            _withdrawPosition = Vector2.zero;
        }
    }
    
    private void EscapeSuck()
    {
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
    }
}
