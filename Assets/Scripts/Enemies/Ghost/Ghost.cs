using System;
using System.Collections;
using Player;
using UnityEngine;
using VContainer;
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
    [SerializeField] private float waitTimeAfterSpikeDeath = 1f;
    [SerializeField] private SmoothDampScript smoothDampScript = new();
    
    [HideInInspector] public bool sucked;
    [HideInInspector] public Vector2 suckVelocity;
    
    private bool _withdrawing;
    private Vector2 _withdrawPosition;
    private Collider2D _targetCollider;
    private bool _playerDiedToSpike;

    private GhostAnimations _ghostAnimations;
    private GravityObjectPlayerMovement _playerMovement;
    
    protected new void Awake()
    {
        base.Awake();
        _ghostAnimations = GetComponent<GhostAnimations>();
        GravityModifier = 0;
    }
    
    protected override void Start()
    {
        base.Start();
        
        var playerRef = ServiceLocator.Get<PlayerScript>();
        
        _playerMovement = playerRef.GetComponent<GravityObjectPlayerMovement>();
        _targetCollider = playerRef.GetComponent<Collider2D>();
        
        // Subscribe to the event
        _playerMovement.OnPlayerSpikeDeath += OnPlayerSpikeReset;
    }
    
    private void OnEnable()
    {
        // Reset the state
        _playerDiedToSpike = false;
        _withdrawing = false;
        _withdrawPosition = Vector2.zero;
        sucked = false;
        _ghostAnimations.PlayDefaultAnimation();
        if (_playerMovement != null) _playerMovement.OnPlayerSpikeDeath += OnPlayerSpikeReset;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        // Unsubscribe from the event
        if (_playerMovement != null) _playerMovement.OnPlayerSpikeDeath -= OnPlayerSpikeReset;
        StopAllCoroutines();
    }

    protected override void ComputeVelocity()
    {
        base.ComputeVelocity();
        
        if (sucked)
        {
            EscapeSuck();
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        
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
        if (sucked || _playerDiedToSpike) return;
        
        if (_withdrawing)
        {
            Withdraw(minWithdrawDistance, maxWithdrawDistance);
        }
        else
        {
            AttackPlayer();
        }
    }
    
    private void AttackPlayer()
    {
        if (!targetObject) return;
        
        MoveToTarget(_targetCollider.bounds.center);
        
        if (Vector2.Distance(transform.position, _targetCollider.bounds.center) < attackDistance)
        {
            _withdrawing = true;
        }
    }
    
    private void Withdraw(float minWithdraw, float maxWithdraw)
    {
        if (_withdrawPosition == Vector2.zero)
        {
            // Get random distance from the player
            Vector2 randomPosition = Vector2.zero;
            while (randomPosition.magnitude < minWithdraw)
            {
                randomPosition = Random.insideUnitCircle * maxWithdraw;
            }

            _withdrawPosition = (Vector2)_targetCollider.bounds.center + randomPosition;
        }
        
        MoveToTarget(_withdrawPosition);
        
        if (Vector2.Distance(transform.position, _withdrawPosition) < 0.5f)
        {
            _withdrawing = false;
            _playerDiedToSpike = false;
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

    private void OnPlayerSpikeReset()
    {
        StartCoroutine(WaitAfterSpikeReset());
    }

    private IEnumerator WaitAfterSpikeReset()
    {
        _playerDiedToSpike = true;
        yield return new WaitForSeconds(waitTimeAfterSpikeDeath);
        smoothDampScript.SetStartVelocity(Vector2.zero);
        _playerDiedToSpike = false;
    }
}
