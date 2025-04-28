using UnityEngine;
using VContainer;

public class GetAchievementOnStart : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private string achievementID = "FIRST";
    
    private IAchievementManager _achievementManager;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        _achievementManager = container.Resolve<IAchievementManager>();
    }

    private void Start() {
        _achievementManager.TryUnlockAchievement(achievementID);
    }
}