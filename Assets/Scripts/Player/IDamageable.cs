using System;
using HealthComponent;
using UnityEngine;

public interface IDamageable {
    void TakeDamage(DamageInstance damageInstance);
}

[Serializable] public class DamageInstance {
    public int damage;
    public Vector2 knockback;
    public float knockbackStrength;
    public int staggerStrength;
    public bool ignoreInvincibility;
    public WhichAttack which;

    public enum WhichAttack {
        UpAttack,
        CrouchAttack,
        DashAttack
    }
        
    public event Action OnHitAction = delegate { };
    public event Action OnKillAction = delegate { };

    public void TriggerOnHitAction() => OnHitAction.Invoke();

    public void TriggerOnKillAction() => OnKillAction.Invoke();
}