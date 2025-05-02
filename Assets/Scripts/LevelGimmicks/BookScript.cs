using System;
using UnityEngine;
using VContainer;

public class BookScript : MonoBehaviour {
    private static readonly int Open = Animator.StringToHash("Open");
    private static readonly int Close = Animator.StringToHash("Close");

    #region ---Fields---
    [SerializeField] private BoxCollider2D triggerCollider;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] DamageInstance damageInstance;
    [SerializeField] private Animator animator;
    private float _carryColliderSize;
    private bool _isClosing = false;
    private IAudioManager _audioManager;
    #endregion
    
    [Inject]
    public void Construct(IAudioManager audioManager) {
        _audioManager = audioManager;
    }

    private void Start() {
        boxCollider.enabled = false;
        _carryColliderSize = triggerCollider.size.y;
    }

    public void OpenThisBook() {
        animator.SetTrigger(Open);
        boxCollider.enabled = true;
        triggerCollider.enabled = false;
        gameObject.tag = "Ground";
        gameObject.layer = LayerMask.NameToLayer("Ground");
    }

    public void CloseThisBook() {
        triggerCollider.enabled = true; animator.SetTrigger(Close);
        boxCollider.enabled = false;
        gameObject.tag = "Untagged";
        gameObject.layer = LayerMask.NameToLayer("Default");
        _audioManager.PlayOneShotAttached("Book_Slam", gameObject);
    }

    public void ActivateHitbox() {
        _isClosing = true;
    }

    public void DeactivateHitbox() {
        _isClosing = false;
    }
    
    private void OnTriggerStay2D(Collider2D other) {
        if (!_isClosing) return;
        if (other.CompareTag("Player")) {
            if (other.TryGetComponent<IDamageable>(out var component)) {
                Vector2 directionToPlayer = (other.transform.position - transform.position).normalized;
                Vector2 knockbackDirection = directionToPlayer.x > 0 ? Vector2.right : Vector2.left;
                
                damageInstance.knockback = (knockbackDirection + Vector2.up * 0.5f).normalized * damageInstance.knockbackStrength;
                component.TakeDamage(damageInstance);
            }
        }
    }
}
