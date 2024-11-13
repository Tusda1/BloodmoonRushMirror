using System;
using PlayerLoopTimer;
using UnityEngine;
using UnityEngine.EventSystems;

public class FlamingSkull : MonoBehaviour, IDamageable {
    #region ---Fields---
    private DamageInstance damageInstance;
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

    public void TakeDamage(DamageInstance damageInstance) {
        flameTimer.StartTimer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isFlaming) return;
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<IDamageable>(out var component))
            {
                component.TakeDamage(damageInstance);
            }
        }
    }

    private void OnValidate()
    {
        if (flameTimer != null) flameTimer.Duration = flameChargeTime;
    }
}
