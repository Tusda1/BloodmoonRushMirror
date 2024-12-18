using System;
using Attributes;
using PlayerLoopTimer;
using UnityEngine;
using UnityEngine.EventSystems;

public class FlamingSkull : MonoBehaviour, IDamageable {
    private static readonly int Ignite = Animator.StringToHash("Ignite");
    private static readonly int Extinguish = Animator.StringToHash("Extinguish");
    [SerializeField] private GameObject particles;
    private ParticleSystem _particleSystem;
    #region ---Fields---
    [SerializeField] DamageInstance damageInstance;
    [SerializeField] private BoxCollider2D boxCollider;
    private bool isFlaming;
    private CountdownTimer flameTimer;
    [SerializeField] private float flameChargeTime;
    [GetOrAddComponent, SerializeField, HideInInspector] private Animator animator;
    #endregion

    private void Start() {
        _particleSystem = particles.GetComponent<ParticleSystem>();
        
        flameTimer = new CountdownTimer(flameChargeTime);
        flameTimer.OnBegin += FlameOff;
        flameTimer.OnComplete += FlameOn;
        FlameOn();
        _particleSystem.Clear();
    }

    private void FlameOn() {
        isFlaming = true;
        boxCollider.enabled = true;
        animator.SetTrigger(Ignite);
    }

    private void FlameOff() {
        isFlaming = false;
        boxCollider.enabled = false;
        flameTimer.StartTimer();
        animator.SetTrigger(Extinguish);
    }

    public void TakeDamage(DamageInstance playerDamageInstance) {
        flameTimer.StartTimer();
        if (playerDamageInstance.knockback.x > 0)
            particles.transform.localScale = new Vector3(1, 1, 1);
        else if (playerDamageInstance.knockback.x < 0)
            particles.transform.localScale = new Vector3(-1, 1, 1);
        PlayParticles();
        flameTimer.StartTimer();
    }

    private void PlayParticles() {
        _particleSystem.Clear();
        _particleSystem.Play();
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
