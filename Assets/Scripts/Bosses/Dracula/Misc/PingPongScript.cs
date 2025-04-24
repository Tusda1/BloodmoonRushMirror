using System;
using FMODUnity;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class PingPongScript : GravityObject, IDamageable
{
    [SerializeField] private DamageInstance projectileDamageInstance;
    
    public Vector2 setVelocity;

    private void OnEnable()
    {
        GravityModifier = 0;
    }

    protected override void ComputeVelocity()
    {
        targetVelocity = setVelocity;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            // set the knockback direction based on the velocity of the projectile
            var knockbackDirection = GetComponent<Rigidbody2D>().linearVelocity.normalized.x > 0 ? Vector2.right : Vector2.left;
            projectileDamageInstance.knockback = knockbackDirection * projectileDamageInstance.knockbackStrength;
        
            other.GetComponent<IDamageable>().TakeDamage(projectileDamageInstance);
            // Play sound
            RuntimeManager.PlayOneShotAttached("event:/Sfx/Boss/Dracula/Drac_pingpong_damage", gameObject);
        
            gameObject.SetActive(false);
        }
    }
    
    public void TakeDamage(DamageInstance damageInstance)
    {
        RuntimeManager.PlayOneShotAttached("event:/Sfx/Boss/Dracula/Drac_pingpong_hit", gameObject);
        setVelocity.x = Mathf.Abs(setVelocity.x) + 5f;
        // If knocked back, reverse direction
        if (damageInstance.knockback.x > 0)
        {
            // Reverse velocity
            setVelocity.x = Mathf.Abs(setVelocity.x);
        }
        else if (damageInstance.knockback.x < 0)
        {
            setVelocity.x = -Mathf.Abs(setVelocity.x);
        }
    }
}
