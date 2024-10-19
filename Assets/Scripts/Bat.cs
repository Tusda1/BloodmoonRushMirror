using AsyncTimers;
using DG.Tweening;
using Fmod;
using Player;
using UnityEngine;
using UnityEngine.Splines;

public class Bat : MonoBehaviour, IDamageable {
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private SplineAnimate splineAnimate;
    [SerializeField] private float yJitter;
    [SerializeField] private float jitterDuration;
    [SerializeField] private bool loopOnlyOnce;
    [SerializeField] private int health = 1;
    [SerializeField] private int damage = 1;
    [SerializeField] private bool doesKnockBack = true;

    [HideInInspector] public RuntimeSet<GameObject> batRuntimeSet;
    public GameObject parentGameObject;

    private void OnDisable() {
        batRuntimeSet.Remove(parentGameObject);

        DOTween.Kill(spriteTransform);
    }

    private void Start() {
        batRuntimeSet.Add(parentGameObject);

        spriteTransform.DOBlendableMoveBy(new Vector3(0, yJitter, 0), jitterDuration)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        if (loopOnlyOnce) {
            var t = new Timer(splineAnimate.Duration);
            t.OnComplete += () => Destroy(parentGameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent<PlayerScript>(out var playerScript)) {
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
    }

    public bool TakeDamage(int damageAmount, Vector3 attackDirection = default) {
        if (health == 0) return true;
        FmodManager.Instance.PlayOneShot("event:/Sfx/Misc/Generic_impact");
        health -= damageAmount;
        if (health <= 0) {
            Destroy(parentGameObject);
            return true;
        }

        return false;
    }
}