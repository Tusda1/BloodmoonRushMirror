using System;
using Animancer;
using UnityEngine;

public class Icicle : GravityObject
{
    [SerializeField] private AnimationClip icicleFall;
    [SerializeField] private AnimationClip icicleImpact;
    
    [SerializeField] private DamageInstance damageInstance;
    
    private AnimancerComponent _animancerComponent;

    private new void Awake()
    {
        base.Awake();
        _animancerComponent = GetComponent<AnimancerComponent>();
    }
    
    private void OnEnable()
    {
        _animancerComponent.Play(icicleFall);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var directionToPlayer = (other.transform.position - transform.position).normalized.x > 0 ? Vector2.right : Vector2.left;
            damageInstance.knockback = directionToPlayer * damageInstance.knockbackStrength;
            other.gameObject.GetComponent<IDamageable>().TakeDamage(damageInstance);
        }
        
        var state = _animancerComponent.Play(icicleImpact);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            gameObject.SetActive(false);
        };
    }
}
