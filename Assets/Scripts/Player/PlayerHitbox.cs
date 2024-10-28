using System;
using UnityEngine;

namespace Player {
    public class PlayerHitbox : MonoBehaviour {
        public DamageInstance damageInstance;

        public event Action OnHitAction = delegate { };
        public event Action OnKillAction = delegate { };

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player") || !other.TryGetComponent<IDamageable>(out var damageable)) return;
            
            var directionToPlayer = (other.transform.position - transform.position).normalized;
            var knockbackDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;
                
            damageInstance.knockback = (knockbackDirection + Vector2.up * 0.5f).normalized * damageInstance.knockbackStrength;

            damageable.TakeDamage(damageInstance);
        }

        public void TriggerOnHitAction() => OnHitAction.Invoke();

        public void TriggerOnKillAction() => OnKillAction.Invoke();
    }
}