using System;
using UnityEngine;

public class BombScript : GravityObject
{
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private DamageInstance damageInstance;
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Ground"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var foundCollider in colliders)
        {
            if (foundCollider.CompareTag("Player"))
            {
                var directionToPlayer = (foundCollider.transform.position - transform.position).normalized.x > 0 ? Vector2.right : Vector2.left;
                damageInstance.knockback = directionToPlayer * damageInstance.knockbackStrength;
                foundCollider.GetComponent<IDamageable>().TakeDamage(damageInstance);
            }
        }
        gameObject.SetActive(false);
    }
}
