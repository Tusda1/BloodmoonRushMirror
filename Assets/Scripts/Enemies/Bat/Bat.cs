using System.Collections;
using DG.Tweening;
using Player;
using UnityEngine;
using UnityEngine.Splines;
using VContainer;

namespace Bat {
    public class Bat : MonoBehaviour, IDamageable {
        #region ---Fields---
        [Header("Spline Path")] public SplineAnimate splineAnimate;
        [SerializeField] private bool loopOnlyOnce;

        [Header("Jitter")] [SerializeField] private Transform spriteTransform;
        [SerializeField] private float yJitter;
        [SerializeField] private float jitterDuration;

        [Header("Health And Damage")]
        public int defaultHealth = 1;
        public int currentHealth = 1;

        [SerializeField] private DamageInstance attackDamageInstance = new() {
            damage = 1
        };

        [SerializeField] private string takeDamageSFX_ID = "Impact_Generic";

        [HideInInspector] public BatSpawner batSpawner;
    
        [Inject] private IAudioManager _audioManager;
        #endregion

        private void OnEnable() {
            StartCoroutine(Initialize());
        }

        private void OnDisable() {
            StopAllCoroutines();
            DOTween.Kill(spriteTransform);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.TryGetComponent<PlayerScript>(out var playerScript)) return;

            var directionToPlayer = (other.transform.position - transform.position).normalized;
            var knockbackDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;

            attackDamageInstance.knockback = (knockbackDirection + Vector2.up * 0.5f).normalized * attackDamageInstance.knockbackStrength;

            playerScript.TakeDamage(attackDamageInstance);
        }

        private IEnumerator Initialize() {
            yield return null;
            spriteTransform.DOBlendableMoveBy(new Vector3(0, yJitter, 0), jitterDuration)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

            if (splineAnimate == null) yield break;

            splineAnimate.Container = batSpawner.batPath;
            splineAnimate.Play();

            if (loopOnlyOnce) {
                StartCoroutine(Despawn());
            }
        }

        private IEnumerator Despawn() {
            yield return new WaitForSeconds(splineAnimate.Duration);
            batSpawner.DespawnBat(this);
        }

        public void TakeDamage(DamageInstance damageInstance) {
            if (currentHealth == 0) return;
            damageInstance.TriggerOnHitAction();
            
            _audioManager.PlayOneShot(takeDamageSFX_ID);
            
            currentHealth -= damageInstance.damage;
            if (currentHealth > 0) return;
            damageInstance.TriggerOnKillAction();

            batSpawner.DespawnBat(this);
        }
    }
}