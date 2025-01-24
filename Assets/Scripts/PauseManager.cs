using System;
using UnityEngine;

public class PauseManager : IPauseManager {
    public float DefaultTimeScale { get; set; }

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
                Time.timeScale = DefaultTimeScale;
            }
        }
    }

    public event Action OnPause = delegate { };
    public event Action OnResume = delegate { };

    private bool _paused;

    public PauseManager(float defaultTimeScale) {
        DefaultTimeScale = defaultTimeScale;
    }

    public void Pause() => Paused = true;
    public void Resume() => Paused = false;
}