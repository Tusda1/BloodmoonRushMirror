using System;
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
    private bool _deflected;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        _animancerComponent = GetComponent<AnimancerComponent>();
        _animancerComponent.Play(baseAnimation);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (rb2d.linearVelocity == Vector2.zero) return;
        
        if (other.gameObject == shooter && !_deflected) return;
        
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            // set the knockback direction based on the velocity of the projectile
            var knockbackDirection = GetComponent<Rigidbody2D>().linearVelocity.normalized.x > 0 ? Vector2.right : Vector2.left;
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
        if (_deflected) return;
        
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
        _deflected = true;
    }
}
