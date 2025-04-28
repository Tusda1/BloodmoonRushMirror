using Attributes;
using SceneCollections;
using UnityEngine;
using VContainer;

public class DrucillaFight : ConversationTrigger {
    #region ---Fields---
    [SerializeField, Required] private SceneCollectionSO nextScene;
    
    private CameraEffects _cameraEffects;
    private IAchievementManager _achievementManager;
    #endregion

    #region ---Initialization---
    [Inject]
    public override void Construct(IObjectResolver container) {
        base.Construct(container);
        
        _cameraEffects = container.Resolve<CameraEffects>();
        _achievementManager = container.Resolve<IAchievementManager>();
    }
    #endregion
    
    public void TriggerFightEndConversation() {
        cutsceneGraph.TriggerUnityEvent("End");
    }

    public void EndLevel() {
        _cameraEffects.FadeLoadScene(nextScene);
        _achievementManager.TryUnlockAchievement(IAchievementManager.AchievementID_TheDungeons);
    }
}