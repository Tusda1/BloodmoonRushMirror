using System;
using Animancer;
using UnityEngine;
using UnityEngine.Serialization;

public class HomingProjectile : GravityObject
{
    [HideInInspector] public GameObject target;
    [HideInInspector] public Vector2 initialDirection;
    public float launchDistance = 3f;
    
    [SerializeField] private float launchSpeed = 5f;
    [SerializeField] private float homingSpeed = 5f;
    [SerializeField] private float startAngle = 90f; // Default angle facing up
    
    private bool _hasLaunched;
    private Vector2 _initialPosition;
    
    private AnimancerComponent _animancerComponent;
    [SerializeField] private AnimationClip projectileAnimation;
    [SerializeField] private DamageInstance damageInstance;
    [SerializeField] private SmoothDampScript smoothDampScript = new();

    private new void Awake()
    {
        base.Awake();
        GravityModifier = 0f;
        
        _animancerComponent = GetComponent<AnimancerComponent>();
        if (projectileAnimation != null) _animancerComponent.Play(projectileAnimation);
    }

    private void OnEnable()
    {
        _initialPosition = transform.position;
    }

    protected override void ComputeVelocity()
    {
        if (target == null) gameObject.SetActive(false);

        if (!_hasLaunched)
        {
            if (Vector2.Distance(_initialPosition, transform.position) > launchDistance)
            {
                _hasLaunched = true;
                smoothDampScript.SetStartVelocity(initialDirection.normalized * launchSpeed);
            }
            else
            {
                var velocityVector = initialDirection.normalized * launchSpeed;
                targetVelocity = velocityVector;
            }
        }
        else
        {
            Vector2 direction = target.transform.position - transform.position;
            smoothDampScript.SmoothDampVector2(direction.normalized * homingSpeed, ref targetVelocity);
        }

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(targetVelocity.y, targetVelocity.x) * Mathf.Rad2Deg + startAngle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(damageInstance);
        }
        else if (other.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
        }
    }
}