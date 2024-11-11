using UnityEngine;

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
        throw new System.NotImplementedException();
    }
}
