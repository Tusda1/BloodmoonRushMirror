using System;
using UnityEngine;
using VContainer;
#if STEAM_BUILD
using Steamworks;
#endif

public class SteamAchievementManager : IAchievementManager {
    public event Action<string> OnAchievementUnlocked = delegate { };

    [Inject]
    public SteamAchievementManager() {
    }
    
    public void UnlockAchievement(string id) {
#if STEAM_BUILD
        if (!SteamManager.Initialized) return;

        SteamUserStats.SetAchievement(id);
        SteamUserStats.StoreStats();
        
        OnAchievementUnlocked.Invoke(id);
#endif
    }

    public bool IsAchievementUnlocked(string id) {
#if STEAM_BUILD
        SteamUserStats.GetAchievement(id, out var achieved);
        return achieved;
#endif
        Debug.LogWarning("NB! NOT CONNECTED TO STEAM");
        return true;
    }

    public void TryUnlockAchievement(string id) {
        if (!IsAchievementUnlocked(id)) {
            UnlockAchievement(id);
        }
    }
}