using System;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;
using VContainer;
using VContainer.Unity;
#if STEAM_BUILD
using System.Threading.Tasks;
using Steamworks;
#endif

public partial class SteamSpeedrunTimer : ISpeedrunTimer, IBind<LevelTimerData>, ITickable, IDisposable {
    #region ---Fields---
    public bool IsRunning { get; set; }
    private bool _timerEnabled;
    private float _time;

    private readonly ISaveLoadSystemManager _saveLoadSystemManager;
    private readonly LevelDataManager _levelDataManager;
    private readonly string _leaderboardID;
#if STEAM_BUILD
    private Task<SteamLeaderboard_t> _fetchLeaderboardTask;
    private SteamLeaderboard_t Leaderboard => _fetchLeaderboardTask.Result;
#endif
    #endregion

    [Inject]
    public SteamSpeedrunTimer(IObjectResolver container) {
        _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
        _levelDataManager = container.Resolve<LevelDataManager>();
        _leaderboardID = container.ResolveInstanceID<string>("LeaderboardID");
    }

    public void Dispose() {
        Unbind();
    }

    public void Tick() {
        if (!_timerEnabled || !IsRunning) return;
        
        _time += Time.deltaTime;
    }

    public void StartTimer() {
        if (_timerEnabled) return;

        Bind();

        _time = 0;
        _timerEnabled = true;
        IsRunning = true;
#if STEAM_BUILD
        _fetchLeaderboardTask = SteamLeaderboardHelper.FindLeaderboardAsync(_leaderboardID);
#endif
    }


    public void StopTimer() {
        if (!_timerEnabled) return;

        _timerEnabled = false;
        var timeInMs = (int)(_time * 1000);

        Debug.Log(MillisecondsFormat(timeInMs));
#if STEAM_BUILD
        if (!_fetchLeaderboardTask.IsCompletedSuccessfully) {
            Debug.LogError("Leaderboard not found. Cannot upload score.");
            return;
        }

        SteamUserStats.UploadLeaderboardScore(
            Leaderboard,
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
            timeInMs,
            null,
            0);
#endif
    }

    public void PauseTimer() => IsRunning = false;

    public void ResumeTimer() => IsRunning = true;

    public void CancelTimer() => _timerEnabled = false;

    private static string MillisecondsFormat(int milliseconds) {
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";
    }
}