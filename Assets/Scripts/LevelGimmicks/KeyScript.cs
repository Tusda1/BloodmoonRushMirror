using System;
using Attributes;
using Player;
using PlayerLoopTimer;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class KeyScript : GravityObject {
    #region ---Fields---
    [SerializeField] private BoxCollider2D boxCollider2D;
    public bool isCollected = false;
    [SerializeField] private Transform followTransform;
    private Vector2 _velocity = Vector2.zero;
    private Vector2 _carryingPosition = Vector2.zero;
    private Vector3 _offset = Vector3.zero;
    [SerializeField] private float smoothTime;
    [SerializeField] private float followSpeed;
    private CountdownTimer _destructionTimer;
    #endregion
    
    protected override void ComputeVelocity() {
        if (!isCollected) return;
        
        var targetPosition = followTransform.position - transform.position;
        
        _carryingPosition = Vector2.SmoothDamp(_carryingPosition, targetPosition + _offset, ref _velocity, smoothTime);
        
        targetVelocity = _carryingPosition * followSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player"))
            if (other.TryGetComponent<PlayerScript>(out var playerScript))
                playerScript.GetKey(gameObject);
    }

    public void StartDestructionTimer(float destructionTime) {
        _destructionTimer = new CountdownTimer(destructionTime);
        _destructionTimer.OnComplete += UseKey;
        _destructionTimer.StartTimer();
    }
    
    public void SetFollowTransform(Transform followTransform, Vector2 offset = default) {
        this.followTransform = followTransform;
        _offset = offset;
    }

    private void UseKey() {
        Destroy(gameObject);
    }
    
}
