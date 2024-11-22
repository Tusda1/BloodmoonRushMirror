using System;
using UnityEngine;

namespace Player {
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerHitbox : MonoBehaviour {
        [SerializeField] private PlayerScript playerScript;

        public DamageInstance damageInstance;

        public event Action OnHitAction = delegate { };
        public event Action OnKillAction = delegate { };

        private void Awake() {
            ServiceLocator.Register(this);
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Player") || !other.TryGetComponent<IDamageable>(out var damageable)) return;

            if (other.CompareTag("Enemy") && !playerScript.playerMovement.wasGroundedLastFrame)
                playerScript.playerMovement.killCharges++;
            
            if (playerScript.transform.localScale.x > 0) {
                damageInstance.knockback.x = Mathf.Abs(damageInstance.knockback.x);
            }
            else {
                damageInstance.knockback.x = -Mathf.Abs(damageInstance.knockback.x);
            }

            damageable.TakeDamage(damageInstance);
        }

        public void TriggerOnHitAction() => OnHitAction.Invoke();

        public void TriggerOnKillAction() => OnKillAction.Invoke();
    }
}