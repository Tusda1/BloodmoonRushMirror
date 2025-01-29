using System;
using UnityEngine;

public class Boomerang : GravityObject
{
    [SerializeField] private float speed;
    
    [HideInInspector] public Vector2 direction;
    [HideInInspector] public bool returned;
    
    private bool _isreturning;
    private Vector2 _initialPosition;
    private Vector2 _turnPosition;
    private Vector2 _moveDirection;
    private Vector2 _velocity;
    
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
        targetVelocity = _moveDirection * speed;
        
        // Send raycast to look for wall in direction
        RaycastHit2D hit = Physics2D.Raycast(position, direction, 100f, LayerMask.GetMask("Ground"));
        if (hit.collider == null) return;

        _turnPosition = hit.point - direction * 2f;
    }

    protected override void ComputeVelocity()
    {
        if (Vector2.Distance(transform.position, _initialPosition) > Vector2.Distance(_turnPosition, _initialPosition))
        {
            _moveDirection = -direction;
            _smoothDampScript.SmoothDampVector2(_moveDirection * speed, ref targetVelocity);
            _isreturning = true;
        }

        if (_isreturning && Vector2.Distance(_turnPosition, transform.position) > Vector2.Distance(_turnPosition, _initialPosition))
        {
            returned = true;
            gameObject.SetActive(false);
        }
        
        targetVelocity = _moveDirection * speed;
    }
}
