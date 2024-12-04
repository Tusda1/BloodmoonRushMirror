using System;
using System.Collections;
using Animancer;
using UnityEngine;

public class ProjectileDeflectable : MonoBehaviour, IDamageable
{
    [SerializeField] private DamageInstance damageInstance;
    [SerializeField] private AnimationClip baseAnimation;
    [SerializeField] private AnimationClip hitAnimation;
    
    private AnimancerComponent _animancerComponent;
    private Rigidbody2D rb2d;
    public GameObject shooter;
    private Vector2 _velocityDirection;
    private bool _isDeflected;

    private float _colliderRadius;
    private CircleCollider2D _circleCollider2D;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        _velocityDirection = rb2d.linearVelocity.normalized;
        
        transform.localScale = _velocityDirection.x > 0 ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
        
        _animancerComponent = GetComponent<AnimancerComponent>();
        _animancerComponent.Play(baseAnimation);
        
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _colliderRadius = _circleCollider2D.radius;
        
        StartCoroutine(EnableCollider());
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
            var knockbackDirection = _velocityDirection.x > 0 ? Vector2.right : Vector2.left;
            damageInstance.knockback = knockbackDirection * damageInstance.knockbackStrength;
            
            damageable.TakeDamage(damageInstance);
        }
        
        if (damageable == null && other.gameObject.layer != LayerMask.NameToLayer("Ground")) return; 
        
        AnimancerState state = _animancerComponent.Play(hitAnimation);
        state.Events(this).OnEnd = () => Destroy(gameObject);
        
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
        
        rb2d.linearVelocity = velocity * 3;
        _velocityDirection = rb2d.linearVelocity.normalized;
    }
}
