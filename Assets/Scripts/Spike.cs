using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class Spike : MonoBehaviour {
    [SerializeField] private DamageInstance damageInstance;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.transform.TryGetComponent<IDamageable>(out var damageable)) {
            switch (damageable) {
                case PlayerScript playerScript:
                    playerScript.GoToLastGroundedPosition();
                    playerScript.TakeDamage(damageInstance);
                    break;
                // case EnemyBase enemyBase:
                //     if (!enemyBase.CanTakeDamageFromSpikes)
                //     {
                //         return;
                //     }
                //     enemyBase.TakeDamage(10);
                //     break;
            }
        }
    }
}