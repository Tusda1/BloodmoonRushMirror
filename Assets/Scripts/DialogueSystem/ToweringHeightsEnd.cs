using Attributes;
using SceneCollections;
using UnityEngine;
using VContainer;

public class ToweringHeightsEnd : ConversationTrigger {
    #region ---Fields---
    [SerializeField, Required] private SceneCollectionSO nextScene;
    
    private CameraEffects _cameraEffects;
    private IAchievementManager _achievementManager;
    private ISpeedrunTimer _speedrunTimer;
    #endregion

    #region ---Initialization---
    [Inject]
    public override void Construct(IObjectResolver container) {
        base.Construct(container);
        
        _cameraEffects = container.Resolve<CameraEffects>();
        _achievementManager = container.Resolve<IAchievementManager>();
        _speedrunTimer = container.Resolve<ISpeedrunTimer>();
    }
    #endregion
    
    public void EndLevel() {
        _cameraEffects.FadeLoadScene(nextScene);
        _achievementManager.TryUnlockAchievement(IAchievementManager.AchievementID_ToweringHeights);
        _speedrunTimer.StopTimer();
    }
}