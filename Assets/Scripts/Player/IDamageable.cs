using UnityEngine;

public interface IDamageable {
    bool TakeDamage(int damageAmount, Vector3 attackDirection = default);
}