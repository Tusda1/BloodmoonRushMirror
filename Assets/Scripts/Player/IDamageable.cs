using System;
using UnityEngine;

public interface IDamageable {
    void TakeDamage(DamageInstance damageInstance);
}

[Serializable] public class DamageInstance {
    public int damage;
    public Vector2 knockbackStrength;
    public Vector2 damageDirection;
    public int staggerStrength;
        
    public event Action OnHitAction = delegate { };
    public event Action OnKillAction = delegate { };

    public virtual void TriggerOnHitAction() => OnHitAction.Invoke();

    public virtual void TriggerOnKillAction() => OnKillAction.Invoke();
}