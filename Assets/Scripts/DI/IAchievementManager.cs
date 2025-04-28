using System;

public interface IAchievementManager {
    event Action<string> OnAchievementUnlocked;

    public const string AchievementID_First = "FIRST";
    public const string AchievementID_EntranceHall = "COMPLETION_ENTRANCEHALL";
    public const string AchievementID_TheDungeons = "COMPLETION_THEDUNGEONS";
    public const string AchievementID_TheBoiler = "COMPLETION_THEBOILER";
    public const string AchievementID_CastleMain = "COMPLETION_CASTLEMAIN";
    public const string AchievementID_ToweringHeights = "COMPLETION_TOWERINGHEIGHTS";
    public const string AchievementID_DraculasOffice = "COMPLETION_DRACULASOFFICE";
    
    void UnlockAchievement(string id);
    bool IsAchievementUnlocked(string id);
    
    void TryUnlockAchievement(string id);
}