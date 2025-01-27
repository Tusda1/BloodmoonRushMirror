using System;
using UnityEngine;

public class PauseManager : IPauseManager {
    #region ---Fields---
    public bool Paused {
        get => _paused;
        set {
            if (_paused == value) return;

            _paused = value;

            if (_paused) {
                OnPause.Invoke();
                Time.timeScale = 0;
            }
            else {
                OnResume.Invoke();
                Time.timeScale = _defaultTimeScale;
            }
        }
    }

    public event Action OnPause = delegate { };
    public event Action OnResume = delegate { };

    private readonly float _defaultTimeScale;
    private bool _paused;
    #endregion

    public PauseManager(float defaultTimeScale) {
        _defaultTimeScale = defaultTimeScale;
    }

    public void Pause() => Paused = true;
    
    public void Resume() => Paused = false;
}