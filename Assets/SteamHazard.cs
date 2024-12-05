using System;
using PlayerLoopTimer;
using Unity.VisualScripting;
using UnityEngine;

public class SteamHazard : MonoBehaviour {
    private static readonly int Steam = Animator.StringToHash("ShootSteam");

    #region ---Fields---
    private CountdownTimer _steamHazardTimer;
    [SerializeField] private float timeBetweenSteam;
    [SerializeField] private Animator animator;
    [SerializeField] private DamageInstance damageInstance;
    [SerializeField] private BoxCollider2D boxCollider2D;
    #endregion

    private void Start() {
        _steamHazardTimer = new CountdownTimer(timeBetweenSteam);
        _steamHazardTimer.OnComplete += ShootSteam;
        boxCollider2D.enabled = false;
    }

    private void ShootSteam() {
        boxCollider2D.enabled = true;
        animator.SetTrigger(Steam);
        boxCollider2D.enabled = true;
    }

    public void DoneShooting() {
        boxCollider2D.enabled = false;
        _steamHazardTimer.StartTimer();
    }
    
    private void OnTriggerStay2D(Collider2D other) {
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
