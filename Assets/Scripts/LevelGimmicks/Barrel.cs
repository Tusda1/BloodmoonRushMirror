using UnityEngine;

public class Barrel : EnemyBase {
    #region ---Fields---
    #endregion

    public new void TakeDamage(DamageInstance damage) {
        if (isGrounded) return;
        base.TakeDamage(damage);
    }
}