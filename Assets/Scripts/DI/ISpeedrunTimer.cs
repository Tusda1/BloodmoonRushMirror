public interface ISpeedrunTimer {
    bool IsRunning { get; set; }
    
    void StartTimer();
    void StopTimer();
    void PauseTimer();
    void ResumeTimer();
    void CancelTimer();
}