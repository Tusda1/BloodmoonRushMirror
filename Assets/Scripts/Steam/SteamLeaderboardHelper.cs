using System;
using System.Threading.Tasks;
using Steamworks;

public static class SteamLeaderboardHelper {
    public static Task<SteamLeaderboard_t> FindLeaderboardAsync(string leaderboardName) {
        var tcs = new TaskCompletionSource<SteamLeaderboard_t>();

        var hSteamAPICall = SteamUserStats.FindLeaderboard(leaderboardName);
        var callResult = CallResult<LeaderboardFindResult_t>.Create();

        callResult.Set(hSteamAPICall, (pCallback, failure) => {
            if (failure || pCallback.m_bLeaderboardFound == 0) {
                tcs.SetException(new Exception($"Failed to find leaderboard: {leaderboardName}"));
            } else {
                tcs.SetResult(pCallback.m_hSteamLeaderboard);
            }
        });

        return tcs.Task;
    }
}