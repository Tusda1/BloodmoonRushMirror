using UnityEngine;

namespace Player {
    public class PlayerHitbox : MonoBehaviour {
        public DamageInstance damageInstance;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player") || !other.TryGetComponent<IDamageable>(out var damageable)) return;

            var direction = (other.transform.position - transform.position).normalized;
            var cardinalDirection = direction.x > 0 ? Vector2.right : Vector2.left;
            
            damageInstance.damageDirection.x = cardinalDirection.x;
            
            damageable.TakeDamage(damageInstance);
        }
    }
}