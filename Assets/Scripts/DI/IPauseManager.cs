using System;

public interface IPauseManager {
    bool Paused { get; set; }

    event Action OnPause;
    event Action OnResume;

    void Pause();
    void Resume();
}