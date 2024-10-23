using System;
using UnityEngine;

namespace Player {
    public class PlayerHitbox : MonoBehaviour {
        public DamageInstance damageInstance;

        public event Action OnHitAction = delegate { };
        public event Action OnKillAction = delegate { };

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player") || !other.TryGetComponent<IDamageable>(out var damageable)) return;

            var direction = (other.transform.position - transform.position).normalized;
            var cardinalDirection = direction.x > 0 ? Vector2.right : Vector2.left;

            damageInstance.damageDirection.x = cardinalDirection.x;

            damageable.TakeDamage(damageInstance);
        }

        public virtual void TriggerOnHitAction() => OnHitAction.Invoke();

        public virtual void TriggerOnKillAction() => OnKillAction.Invoke();
    }
}