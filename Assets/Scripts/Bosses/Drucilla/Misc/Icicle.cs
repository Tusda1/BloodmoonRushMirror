using System;
using UnityEngine;

public class Icicle : GravityObject
{
    [SerializeField] private DamageInstance damageInstance;
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var directionToPlayer = (other.transform.position - transform.position).normalized.x > 0 ? Vector2.right : Vector2.left;
            damageInstance.knockback = directionToPlayer * damageInstance.knockbackStrength;
            other.gameObject.GetComponent<IDamageable>().TakeDamage(damageInstance);
            gameObject.SetActive(false);
        }
        else if (other.gameObject.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
        }
    }
}
