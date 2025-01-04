using Steamworks;
using UnityEngine;

public class FirstTimeAchievement : MonoBehaviour {
    #region ---Fields---
    private const string TestAchievementName = "First";
    #endregion

    private void Start() {
        if (!SteamManager.Initialized) return;

#if UNITY_EDITOR || DEBUG
        SteamUserStats.ResetAllStats(true);
        
        Debug.Log("Resetting stats for: " + SteamFriends.GetPersonaName());
#endif
        
        SteamUserStats.GetAchievement(TestAchievementName, out var achieved);

        if (!achieved) {
            SteamUserStats.SetAchievement(TestAchievementName);
            SteamUserStats.StoreStats();
        }
    }
}