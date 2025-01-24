using System;

public interface IPauseManager {
    public float DefaultTimeScale { get; set; }
    bool Paused { get; set; }

    event Action OnPause;
    event Action OnResume;

    void Pause();
    void Resume();
}