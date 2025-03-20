using System;
using System.Collections;
using UnityEngine;

public class Boomerang : GravityObject
{
    [SerializeField] private float speed;
    [SerializeField] private float catchDistance;
    [SerializeField] private DamageInstance damageInstance;
    
    [HideInInspector] public Vector2 direction;
    [HideInInspector] public bool returned;
    
    private bool _isReturning;
    private Vector2 _initialPosition;
    private Vector2 _turnPosition;
    private Vector2 _floorPosition;
    private Vector2 _moveDirection;
    private Vector2 _velocity;
    private Vector2 _verticalVelocity;
    private float _downVelocity;
    private float _upVelocity;
    
    private readonly SmoothDampScript _smoothDampScript = new();

    private new void Awake()
    {
        base.Awake();
        
        GravityModifier = 0f;
    }

    private void OnEnable()
    {
        returned = false;

        var position = transform.position;
        _initialPosition = position;
        _moveDirection = direction;
        
        // Send raycast to look for wall in direction
        RaycastHit2D hit = Physics2D.Raycast(position, direction, 100f, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            Debug.LogError("Couldn't find wall");
            return;
        }
        
        _turnPosition = hit.point - direction * 2f;
        
        hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            Debug.LogError("Couldn't find ground");
            return;
        }
        
        _floorPosition = hit.point + Vector2.up * 0.5f;

        var timeBeforeSwap = (Mathf.Abs(_initialPosition.x - _turnPosition.x) * 1.5f / speed);
        _downVelocity = -Mathf.Abs(_initialPosition.y - _floorPosition.y) / timeBeforeSwap;
        _upVelocity = -_downVelocity * 3f;
        StartCoroutine(SwapVerticalVelocity(timeBeforeSwap));
    }

    protected override void ComputeVelocity()
    {
        if (Vector2.Distance(transform.position, _initialPosition) > Vector2.Distance(_turnPosition, _initialPosition))
        {
            _moveDirection = -direction;
            _isReturning = true;
        }

        if (_isReturning && Vector2.Distance(_turnPosition, transform.position) > Vector2.Distance(_turnPosition, _initialPosition) - catchDistance)
        {
            _isReturning = false;
            returned = true;
            gameObject.SetActive(false);
        }
        
        _smoothDampScript.SmoothDampVector2(new Vector2(_moveDirection.x * speed, _verticalVelocity.y), ref targetVelocity);
    }
    
    private IEnumerator SwapVerticalVelocity(float timeBeforeSwap)
    {
        _verticalVelocity = new Vector2(_verticalVelocity.x, _downVelocity);
        yield return new WaitForSeconds(timeBeforeSwap);
        _verticalVelocity = new Vector2(_verticalVelocity.x, _upVelocity);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector2 directionToPlayer = (other.transform.position - transform.position).normalized;
            Vector2 knockbackDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;

            damageInstance.knockback = (knockbackDirection + Vector2.up * 0.5f).normalized * damageInstance.knockbackStrength;
            damageable.TakeDamage(damageInstance);
        }
    }
}
