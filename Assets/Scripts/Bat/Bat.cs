using System.Collections;
using DG.Tweening;
using FMODScripts;
using Player;
using UnityEngine;
using UnityEngine.Splines;

namespace Bat {
    public class Bat : MonoBehaviour, IDamageable {
        #region ---Fields---
        [Header("Spline Path")] public SplineAnimate splineAnimate;
        [SerializeField] private bool loopOnlyOnce;

        [Header("Jitter")] [SerializeField] private Transform spriteTransform;
        [SerializeField] private float yJitter;
        [SerializeField] private float jitterDuration;

        [Header("Health And Damage")] [SerializeField]
        private int health = 1;

        [SerializeField] private DamageInstance attackDamageInstance = new() {
            damage = 1
        };

        [SerializeField] private string takeDamageSfx = "Misc/Generic_impact";

        [HideInInspector] public BatSpawner batSpawner;

        private static FMODManager FMODManager => FMODManager.Instance;
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

        public void TakeDamage(DamageInstance enemyDamageInstance) {
            if (health == 0) return;
            enemyDamageInstance.TriggerOnHitAction();

            FMODManager.PlaySfx(takeDamageSfx);
            health -= enemyDamageInstance.damage;
            if (health > 0) return;
            enemyDamageInstance.TriggerOnKillAction();

            batSpawner.DespawnBat(this);
        }
    }
}