using Player;
using UnityEngine;

public class Spike : MonoBehaviour {
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.transform.TryGetComponent<IDamageable>(out var damageable)) {
            switch (damageable) {
                case PlayerScript playerScript:
                    playerScript.GoToLastGroundedPosition();
                    playerScript.TakeDamage(damage);
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