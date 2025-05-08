using System;
using PlayerLoopTimer;
using UnityEngine;
using VContainer;

public class MovingSpikes : MonoBehaviour {
    #region ---Fields---

    [SerializeField] private float upSpeed;
    [SerializeField] private float downSpeed;
    [SerializeField] private float upWait;
    [SerializeField] private float downWait;
    [SerializeField] private bool startDown;
    private float _step;
    private Vector2 _targetPos;
    private Vector2 _downPosition;
    private Vector2 _startPosition;
    private CountdownTimer _upTimer;
    private CountdownTimer _downTimer;
    private CountdownTimer _bufferTimer;

    private IAudioManager _audioManager;
    #endregion

    [Inject]
    public void Construct(IAudioManager audioManager) {
        _audioManager = audioManager;
    }
    
    private void Awake() {
        _downPosition = new Vector2(transform.position.x, transform.position.y - 1); ;
        _startPosition = new Vector2(transform.position.x, transform.position.y);

        if (!startDown) {
            _upTimer = new CountdownTimer(upWait);
            _downTimer = new CountdownTimer(downWait);
        }
        else {
            _upTimer = new CountdownTimer(downWait);
            _downTimer = new CountdownTimer(upWait);
        }

        _upTimer.OnComplete += Down;
        _upTimer.OnComplete += () => {_downTimer.StartTimer();};
        _downTimer.OnComplete += Up;
        _downTimer.OnComplete += () => {_upTimer.StartTimer();};
    }

    private void OnEnable() {
        if (startDown) {
            transform.position = _downPosition;
            _downTimer.StartTimer();
        }
        else {
            _upTimer.StartTimer();
        }
    }

    private void Update() {
        transform.position = Vector2.MoveTowards(transform.position, _targetPos, _step);
    }

    private void Up() {
        _audioManager.PlayOneShotAttached("Spike_Up", gameObject);
        _step = upSpeed * Time.deltaTime;
        _targetPos = new Vector2(_startPosition.x, _startPosition.y);
    }

    private void Down() {
        _audioManager.PlayOneShotAttached("Spike_Down", gameObject);
        _step = downSpeed * Time.deltaTime;
        _targetPos = new Vector2(_downPosition.x, _downPosition.y);
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (_upTimer != null) _upTimer.Duration = upWait;
        if (_downTimer != null) _downTimer.Duration = downWait;
    }
#endif
}
