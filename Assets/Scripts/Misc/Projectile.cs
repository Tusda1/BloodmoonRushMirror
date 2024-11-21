using UnityEngine;

public class Projectile : MonoBehaviour, IDamageable
{
    [SerializeField] private DamageInstance damageInstance;
    
    public GameObject shooter;
    
    private void OnTriggerEnter2D(Collider2D other) 
    {
        
        if (other.gameObject == shooter) return;

        // if the object collided with ground, destroy gameobject
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) { Destroy(gameObject); return; }
        
        if (!other.TryGetComponent<IDamageable>(out var damageable)) { return; }
        
        // set the knockback direction based on the velocity of the projectile
        var knockbackDirection = GetComponent<Rigidbody2D>().linearVelocity.normalized.x > 0 ? Vector2.right : Vector2.left;
        damageInstance.knockback = knockbackDirection * damageInstance.knockbackStrength;
        
        damageable.TakeDamage(damageInstance);
        
        Destroy(gameObject);
    }
    
    public void TakeDamage(DamageInstance damageInstance)
    {
        Destroy(gameObject);
    }
}
