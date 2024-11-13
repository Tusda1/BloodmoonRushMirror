using System;
using PlayerLoopTimer;
using UnityEngine;
using UnityEngine.EventSystems;

public class FlamingSkull : MonoBehaviour, IDamageable {
    #region ---Fields---
    [SerializeField] DamageInstance damageInstance;
    [SerializeField] private BoxCollider2D boxCollider;
    private bool isFlaming;
    private CountdownTimer flameTimer;
    [SerializeField] private float flameChargeTime;
    [SerializeField] private SpriteRenderer flameRenderer;
    #endregion

    private void Start() {
        flameTimer = new CountdownTimer(flameChargeTime);
        flameTimer.OnBegin += FlameOff;
        flameTimer.OnComplete += FlameOn;
        FlameOn();
    }

    private void FlameOn() {
        isFlaming = true;
        //change animation
        flameRenderer.color = Color.red;
    }

    private void FlameOff() {
        isFlaming = false;
        flameTimer.StartTimer();
        //change animation
        flameRenderer.color = new Color(1f, 1f, 1f, 0.2f);
    }

    public void TakeDamage(DamageInstance playerDamageInstance) {
        flameTimer.StartTimer();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!isFlaming) return;
        if (other.CompareTag("Player")) {
            if (other.TryGetComponent<IDamageable>(out var component)) {
                Vector2 directionToPlayer = (other.transform.position - transform.position).normalized;
                Vector2 knockbackDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;
                
                damageInstance.knockback = (knockbackDirection + Vector2.up * 0.5f).normalized * damageInstance.knockbackStrength;
                component.TakeDamage(damageInstance);
            }
        }
    }

    private void OnValidate() {
        if (flameTimer != null) flameTimer.Duration = flameChargeTime;
    }
}
