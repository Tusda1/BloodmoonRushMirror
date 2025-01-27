using System;

public interface IAchievementManager {
    event Action<string> OnAchievementUnlocked;
    
    void UnlockAchievement(string id);
    bool IsAchievementUnlocked(string id);
}