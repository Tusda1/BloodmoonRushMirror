using System;
using UnityEngine;

namespace Player {
    public class PlayerHitbox : MonoBehaviour {
        public int damage;
        public event Action OnHitAction = delegate { };

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player")) return;

            if (other.TryGetComponent<IDamageable>(out var damageable)) {
                var direction = (other.transform.position - transform.position).normalized;

                // Determine if the direction is closer to Vector2.right or Vector2.left
                Vector2 cardinalDirection = direction.x > 0 ? Vector2.right : Vector2.left;
                
                if (damageable.TakeDamage(damage, cardinalDirection)) {
                    OnHitAction.Invoke();
                }

                Debug.Log($"Damaging for {damage} damage");
            }
        }
    }
}