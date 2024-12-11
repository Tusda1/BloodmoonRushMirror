using System;
using HealthComponent;
using UnityEngine;

namespace Player {
    [Serializable] public class PlayerHealth : HealthComponentInt {
        [SerializeField] private int tempHealth;
        public int maxTempHealth = 7;

        public int TotalHealth => Health + TempHealth;

        public int TempHealth {
            get => tempHealth;
            set {
                tempHealth = Math.Clamp(value, 0, maxTempHealth);
                InvokeHealthChangedEvent();
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            Health = health;
            health = Health;
            TempHealth = tempHealth;
            tempHealth = TempHealth;
        }
#endif

        public void TakeDamage(int amount, bool ignoreInvincibilityFrames) {
            if (HasInvincibilityFrames) return;

            var tempHealthIfSubtracted = TempHealth - amount;

            if (tempHealthIfSubtracted < 0) {
                TempHealth = 0;
                amount = -tempHealthIfSubtracted;
            }
            else {
                TempHealth -= amount;
                amount = 0;
            }

            base.TakeDamage(amount);
        }

        public static implicit operator int(PlayerHealth playerHealth) {
            return playerHealth.TotalHealth;
        }
    }
}