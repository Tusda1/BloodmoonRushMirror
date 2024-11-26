using System;
using PlayerLoopTimer;
using UnityEngine;

public class RetractingPlatformScript : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private BoxCollider2D triggerCollider;
    [SerializeField] private float retractingTime;
    [SerializeField] private float backUpTime;
    [SerializeField] private Color solidColor;
    [SerializeField] private Color retractedColor; //colors are test stuff before assets
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool _retracted;
    private bool _readyToRetract = false;
    private bool _playerIsInFront;
    private CountdownTimer _retractingTimer;
    private CountdownTimer _backUpTimer;
    #endregion

    private void Start() {
        _retractingTimer = new CountdownTimer(retractingTime);
        _backUpTimer = new CountdownTimer(backUpTime);
        _retractingTimer.OnComplete += Retract;
        _backUpTimer.OnComplete += BackUp;
        
        spriteRenderer.color = solidColor; //test
    }

    private void Retract() {
        boxCollider.enabled = false;
        spriteRenderer.color = retractedColor; //test
        _retracted = true;
        //do animation here hihihehe
        _backUpTimer.StartTimer();
    }

    private void BackUp() {
        if (_playerIsInFront) {
            _readyToRetract = true;
            return;
        }
        boxCollider.enabled = true;
        spriteRenderer.color = solidColor; //test
        _retracted = false;
        //do animation here hihi
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
