using System;
using PlayerLoopTimer;
using UnityEngine;
using VContainer;

public class RetractingPlatformScript : MonoBehaviour {
    private static readonly int Up = Animator.StringToHash("Up");
    private static readonly int Down = Animator.StringToHash("Down");

    #region ---Fields---
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private BoxCollider2D triggerCollider;
    [SerializeField] private float retractingTime;
    [SerializeField] private float backUpTime;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    private bool _retracted;
    private bool _readyToRetract = false;
    private bool _playerIsInFront;
    private CountdownTimer _retractingTimer;
    private CountdownTimer _backUpTimer;
    private IAudioManager _audioManager;

    #endregion
    
    [Inject]
    public void Construct(IAudioManager audioManager) {
        _audioManager = audioManager;
    }

    private void Start() {
        _retractingTimer = new CountdownTimer(retractingTime);
        _backUpTimer = new CountdownTimer(backUpTime);
        _retractingTimer.OnComplete += Retract;
        _backUpTimer.OnComplete += BackUp;
    }

    private void Retract() {
        boxCollider.enabled = false;
        animator.SetTrigger(Down);
        _retracted = true;
        _audioManager.PlayOneShot("Falling_Retract");
        _backUpTimer.StartTimer();
    }

    private void BackUp() {
        if (_playerIsInFront) {
            _readyToRetract = true;
            return;
        }
        boxCollider.enabled = true;
        animator.SetTrigger(Up);
        _retracted = false;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            _retractingTimer.StartTimer();
            _playerIsInFront = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            _retractingTimer.StopTimer();
            _playerIsInFront = false;
            if (_readyToRetract) {
                BackUp();
                _readyToRetract = false;
            }
        }
    }
    
#if UNITY_EDITOR

    private void OnValidate() {
        if (_retractingTimer != null) _retractingTimer.Duration = retractingTime;
        if (_backUpTimer != null) _backUpTimer.Duration = backUpTime;
    }

#endif
}
