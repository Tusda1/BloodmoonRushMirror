using Player;
using UnityEngine;

public class SpikeScript : MonoBehaviour {
    [SerializeField] private DamageInstance damageInstance = new();

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.transform.TryGetComponent<IDamageable>(out var damageable)) return;
        
        switch (damageable) {
            case PlayerScript playerScript:
                playerScript.TakeDamage(damageInstance);
                playerScript.playerMovement.SpikeReset();
                break;
            case EnemyBase enemyBase:
                damageInstance.damage = 99;
                enemyBase.TakeDamage(damageInstance);
                damageInstance.damage = 1;
                break;
        }
    }
}