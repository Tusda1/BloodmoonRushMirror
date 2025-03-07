using System;
using PlayerLoopTimer;
using Unity.VisualScripting;
using UnityEngine;

public class SteamHazard : MonoBehaviour {
    private static readonly int Steam = Animator.StringToHash("ShootSteam");

    #region ---Fields---
    private CountdownTimer _steamHazardTimer;
    private CountdownTimer _hurtboxDelayTimer;
    private CountdownTimer _steamDurationTimer;
    [SerializeField] private float timeBetweenSteam;
    [SerializeField] private float hurtboxDelayTime;
    [SerializeField] private float hurtboxTime;
    [SerializeField] private Animator animator;
    [SerializeField] private DamageInstance damageInstance;
    [SerializeField] private BoxCollider2D boxCollider2D;
    #endregion

    private void Start() {
        _steamHazardTimer = new CountdownTimer(timeBetweenSteam);
        _hurtboxDelayTimer = new CountdownTimer(hurtboxDelayTime);
        _steamDurationTimer = new CountdownTimer(hurtboxTime);
        _steamHazardTimer.OnComplete += ShootSteam;
        _hurtboxDelayTimer.OnComplete += EnableCollider;
        _steamDurationTimer.OnComplete += DisableCollider;
        boxCollider2D.enabled = false;
    }

    private void ShootSteam() {
        animator.SetTrigger(Steam);
        _hurtboxDelayTimer.StartTimer();
    }

    private void EnableCollider() {
        boxCollider2D.enabled = true;
        _steamDurationTimer.StartTimer();
    }

    private void DisableCollider() {
        boxCollider2D.enabled = false;
    }

    public void DoneShooting() {
        _steamHazardTimer.StartTimer();
    }
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            if (other.TryGetComponent<IDamageable>(out var component)) {
                Vector2 directionToPlayer = (other.transform.position - transform.position).normalized;
                Vector2 knockbackDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;
                
                damageInstance.knockback = (knockbackDirection + Vector2.up * 0.5f).normalized * damageInstance.knockbackStrength;
                component.TakeDamage(damageInstance);
            }
        }
    }
    
#if UNITY_EDITOR
    
    private void OnValidate() {
    if (_steamHazardTimer != null) _steamHazardTimer.Duration = timeBetweenSteam;
    }
    
#endif
}
