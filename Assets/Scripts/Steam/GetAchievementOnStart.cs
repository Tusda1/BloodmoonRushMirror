using UnityEngine;
using VContainer;

public class GetAchievementOnStart : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private string achievementId = "First";
    
    [Inject] private IAchievementManager _achievementManager;
    #endregion

    private void Start() {
        if (!_achievementManager.IsAchievementUnlocked(achievementId)) {
            _achievementManager.UnlockAchievement(achievementId);
        }
    }
}