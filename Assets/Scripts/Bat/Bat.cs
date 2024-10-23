using System.Collections;
using DG.Tweening;
using Fmod;
using Player;
using UnityEngine;
using UnityEngine.Splines;

namespace Bat {
    public class Bat : MonoBehaviour, IDamageable {
        [Header("Spline Path")] public SplineAnimate splineAnimate;
        [SerializeField] private bool loopOnlyOnce;

        [Header("Jitter")] [SerializeField] private Transform spriteTransform;
        [SerializeField] private float yJitter;
        [SerializeField] private float jitterDuration;

        [Header("Health And Damage")] [SerializeField]
        private int health = 1;

        [SerializeField] private int damage = 1;
        [SerializeField] private string takeDamageSfx = "Misc/Generic_impact";
        [SerializeField] private bool doesKnockBack = true;

        [HideInInspector] public BatSpawner batSpawner;

        private static FmodManager fmodManager => FmodManager.Instance;

        private void OnEnable() {
            StartCoroutine(Initialize());
        }

        private void OnDisable() {
            StopAllCoroutines();
            DOTween.Kill(spriteTransform);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.TryGetComponent<PlayerScript>(out var playerScript)) return;

            var dir = (playerScript.transform.position - transform.position).normalized;
            if (doesKnockBack) {
                if (damage != 0) {
                    playerScript.TakeDamage(damage, dir);
                }
            }
            else {
                if (damage != 0) {
                    playerScript.TakeDamage(damage);
                }
            }
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

        public bool TakeDamage(int damageAmount, Vector3 attackDirection = default) {
            if (health == 0) return true;

            fmodManager.PlaySfx(takeDamageSfx);
            health -= damageAmount;
            if (health > 0) return false;

            batSpawner.DespawnBat(this);
            return true;
        }
    }
}