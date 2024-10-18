using AsyncTimers;
using UnityEngine;

public class Platform : MonoBehaviour {
    [SerializeField] private BoxCollider2D platformCollider;

    private Timer _platformDisabledTimer;

    private void Start() {
        _platformDisabledTimer = new Timer(0.5f, false);
        _platformDisabledTimer.OnBegin += () => platformCollider.enabled = false;
        _platformDisabledTimer.OnComplete += () => platformCollider.enabled = true;
    }

    private void OnDestroy() {
        _platformDisabledTimer?.CancelTimer();
    }

    public void DisablePlatformCollider() {
        _ = _platformDisabledTimer.StartTimer();
    }
}