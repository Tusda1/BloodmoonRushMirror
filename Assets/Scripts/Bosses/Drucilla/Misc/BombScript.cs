using System;
using Animancer;
using FMODUnity;
using UnityEngine;

public class BombScript : GravityObject
{
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private DamageInstance damageInstance;
    [SerializeField] private AnimationClip bombAnimation;
    [SerializeField] private AnimationClip explosionAnimation;
    
    private AnimancerComponent _animancerComponent;

    private new void Awake()
    {
        base.Awake();
        _animancerComponent = GetComponent<AnimancerComponent>();
    }
    
    private void OnEnable()
    {
        _animancerComponent.Play(bombAnimation);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Ground"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Play explosion sound
        
        RuntimeManager.PlayOneShotAttached("event:/Sfx/Boss/Drucilla/Druc_bomb_explode", gameObject);
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Player"));
        Collider2D[] colliders = new Collider2D[10]; // Adjust the size as needed
        int colliderCount = Physics2D.OverlapCircle(transform.position, explosionRadius, contactFilter, colliders);
        for (int i = 0; i < colliderCount; i++)
        {
            if (colliders[i].CompareTag("Player"))
            {
                var directionToPlayer = (colliders[i].transform.position - transform.position).normalized.x > 0 ? Vector2.right : Vector2.left;
                damageInstance.knockback = directionToPlayer * damageInstance.knockbackStrength;
                colliders[i].GetComponent<IDamageable>().TakeDamage(damageInstance);
            }
        }
        var state = _animancerComponent.Play(explosionAnimation);
        state.Events(this).OnEnd = () =>
        {
            gameObject.SetActive(false);
        };
    }
}
