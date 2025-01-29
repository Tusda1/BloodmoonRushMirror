using System;
using UnityEngine;
using VContainer.Unity;
#if STEAM_BUILD
using System.Threading.Tasks;
using Steamworks;
#endif

public class SteamSpeedrunTimer : ISpeedrunTimer, ITickable {
    #region ---Fields---
    private string _leaderboardID;
    private float _time;
    private bool _timerIsRunning;
    
#if STEAM_BUILD
    private Task<SteamLeaderboard_t> _fetchLeaderboardTask;
    private SteamLeaderboard_t Leaderboard => _fetchLeaderboardTask.Result;
#endif
    #endregion

    public SteamSpeedrunTimer(string leaderboardID) {
        _leaderboardID = leaderboardID;

        if (!string.IsNullOrWhiteSpace(leaderboardID)) {
            StartTimer();
        }
    }

    public void Tick() {
        if (_timerIsRunning) {
            _time += Time.deltaTime;
        }
    }

    public void StartTimer() {
        if (_timerIsRunning) return;
        
        _time = 0;
        _timerIsRunning = true;
#if STEAM_BUILD
        _fetchLeaderboardTask = SteamLeaderboardHelper.FindLeaderboardAsync(leaderboardID);
#endif
    }

    public void StopTimer() {
        if (!_timerIsRunning) return;
        
        _timerIsRunning = false;
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

    public void CancelTimer() {
        _timerIsRunning = false;
    }

    private static string MillisecondsFormat(int milliseconds) {
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";
    }
}