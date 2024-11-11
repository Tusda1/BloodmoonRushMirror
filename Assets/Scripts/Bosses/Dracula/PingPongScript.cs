using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class PingPongScript : MonoBehaviour, IDamageable
{
    private DamageInstance projectileDamageInstance;
    
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            // set the knockback direction based on the velocity of the projectile
            var knockbackDirection = GetComponent<Rigidbody2D>().linearVelocity.normalized.x > 0 ? Vector2.right : Vector2.left;
            projectileDamageInstance.knockback = knockbackDirection * projectileDamageInstance.knockbackStrength;
        
            damageable.TakeDamage(projectileDamageInstance);
        
            Destroy(gameObject);
        }
    }
    
    public void TakeDamage(DamageInstance damageInstance)
    {
        Vector2 velocity = GetComponent<Rigidbody2D>().linearVelocity;
        
        // If knocked back, reverse direction
        if (damageInstance.knockback.x > 0)
        {
            // Reverse velocity
            velocity.x = -Mathf.Abs(velocity.x);
        }
        else if (damageInstance.knockback.x < 0)
        {
            velocity.x = Mathf.Abs(velocity.x);
        }
    }
}
