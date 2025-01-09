#if STEAM_BUILD
using Steamworks;
#endif
using UnityEngine;

public class FirstTimeAchievement : MonoBehaviour {
    #region ---Fields---
    private const string TestAchievementName = "First";
    #endregion

    private void Start() {
#if STEAM_BUILD
        if (!SteamManager.Initialized) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        SteamUserStats.ResetAllStats(true);
        
        Debug.Log("Resetting stats for: " + SteamFriends.GetPersonaName());
#endif
        
        SteamUserStats.GetAchievement(TestAchievementName, out var achieved);

        if (!achieved) {
            SteamUserStats.SetAchievement(TestAchievementName);
            SteamUserStats.StoreStats();
        }
#endif
    }
}