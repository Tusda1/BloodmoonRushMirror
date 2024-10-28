using Player;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Spike : MonoBehaviour {
    [SerializeField] private DamageInstance damageInstance;

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.transform.TryGetComponent<IDamageable>(out var damageable)) return;
        
        switch (damageable) {
            case PlayerScript playerScript:
                playerScript.TakeDamage(damageInstance);
                playerScript.HitSpike();
                break;
            case EnemyBase enemyBase:
                enemyBase.TakeDamage(damageInstance);
                break;
        }
    }
}