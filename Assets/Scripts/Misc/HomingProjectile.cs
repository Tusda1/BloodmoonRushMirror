using System;
using UnityEngine;

public class HomingProjectile : MonoBehaviour 
{
    [HideInInspector] public GameObject target;
    [HideInInspector] public Vector2 initialDirection;
    public float launchDistance = 3f;
    
    [SerializeField] private float launchSpeed = 5f;
    [SerializeField] private float homingSpeed = 5f;
    
    private bool _hasLaunched;
    private Vector2 _initialPosition;
    private Vector2 _currentVelocity;
    
    private Rigidbody2D _rb2d;
    private SmoothDampScript _smoothDampScript = new();

    private void Awake()
    {
        _rb2d = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _rb2d.linearVelocity = initialDirection.normalized * launchSpeed;
        _initialPosition = transform.position;
    }
    
    private void Update()
    {
        if (target == null) return;

        if (!_hasLaunched && Vector2.Distance(_initialPosition, transform.position) > launchDistance)
        {
            _hasLaunched = true;
        }

        if (_hasLaunched)
        {
            // Create a vector pointing from the current position to the target position
            Vector2 direction = target.transform.position - transform.position;
            _smoothDampScript.SmoothDampVector2(direction.normalized * homingSpeed, ref _currentVelocity);
        }
        
        _rb2d.linearVelocity = _currentVelocity;
    }
}