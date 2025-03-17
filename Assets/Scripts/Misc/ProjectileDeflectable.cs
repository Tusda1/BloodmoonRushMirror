using System;
using System.Collections;
using Animancer;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileDeflectable : MonoBehaviour, IDamageable
{
    [SerializeField] private DamageInstance damageInstance;
    [SerializeField] private AnimationClip baseAnimation;
    [SerializeField] private AnimationClip hitAnimation;
    [SerializeField] private float speed;
    
    [HideInInspector] public GameObject shooter;
    [HideInInspector] public Vector2 velocityDirection;
    
    private AnimancerComponent _animancerComponent;
    private Rigidbody2D rb2d;
    private bool _isDeflected;
    private GameObject _spawnRoom;

    private float _colliderRadius;
    private CircleCollider2D _circleCollider2D;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        
        _animancerComponent = GetComponent<AnimancerComponent>();
        OptionalWarning.EventPlayMismatch.Disable();
        
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _colliderRadius = _circleCollider2D.radius;
    }

    private void OnEnable()
    {
        _spawnRoom = VirtualCameraSwitch.ActiveVirtualCameraSwitch?.gameObject;
        _circleCollider2D.enabled = false;
        
        _animancerComponent.Play(baseAnimation);
        
        StartCoroutine(EnableCollider());
        
        rb2d.linearVelocity = velocityDirection * speed;
        
        transform.localScale = velocityDirection.x > 0 ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        
        _isDeflected = false;
    }

    private void Update()
    {
        if (_spawnRoom != VirtualCameraSwitch.ActiveVirtualCameraSwitch.gameObject)
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator EnableCollider()
    {
        LayerMask layerMask = Physics2D.GetLayerCollisionMask(gameObject.layer);

        while (!_circleCollider2D.enabled)
        {
            // Create an overlap circle to check if the projectile is colliding with anything. If it isn't, enable the collider
            Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, _colliderRadius, layerMask);
            bool hasShooterCollision = false;

            foreach (var collider in results)
            {
                if (collider.gameObject == shooter)
                {
                    hasShooterCollision = true;
                    break;
                }
            }

            if (!hasShooterCollision)
            {
                _circleCollider2D.enabled = true;
            }

            yield return null;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject == shooter && !_isDeflected) return;
        
        if (other.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            // set the knockback direction based on the velocity of the projectile
            var knockbackDirection = velocityDirection.x > 0 ? Vector2.right : Vector2.left;
            damageInstance.knockback = knockbackDirection * damageInstance.knockbackStrength;
            
            damageable.TakeDamage(damageInstance);
        }
        
        if (damageable == null && other.gameObject.layer != LayerMask.NameToLayer("Ground")) return; 
        
        AnimancerState state = _animancerComponent.Play(hitAnimation);
        state.Events(this).OnEnd = () => gameObject.SetActive(false);
        _circleCollider2D.enabled = false;
        
        rb2d.linearVelocity = Vector2.zero;
    }
    
    public void TakeDamage(DamageInstance damageInstance)
    {
        if (_isDeflected) return;
        
        _isDeflected = true;
        
        Vector2 velocity = rb2d.linearVelocity;
        
        // If knocked back, reverse direction
        if (damageInstance.knockback.x > 0)
        {
            // Reverse velocity
            velocity.x = Mathf.Abs(velocity.x);
        }
        else if (damageInstance.knockback.x < 0)
        {
            velocity.x = -Mathf.Abs(velocity.x);
        }

        velocity.y = 0;
        
        rb2d.linearVelocity = velocity * 3;
        velocityDirection = rb2d.linearVelocity.normalized;
    }
}
