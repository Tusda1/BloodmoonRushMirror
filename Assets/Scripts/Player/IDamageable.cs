using System;
using UnityEngine;

public interface IDamageable {
    void TakeDamage(DamageInstance enemyDamageInstance);
}

[Serializable] public class DamageInstance {
    public int damage;
    public Vector2 knockback;
    public float knockbackStrength;
    public int staggerStrength;
        
    public event Action OnHitAction = delegate { };
    public event Action OnKillAction = delegate { };

    public void TriggerOnHitAction() => OnHitAction.Invoke();

    public void TriggerOnKillAction() => OnKillAction.Invoke();
}