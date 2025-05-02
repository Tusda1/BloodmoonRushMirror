using Attributes;
using SceneCollections;
using UnityEngine;
using VContainer;

public class DrucillaFight : ConversationTrigger {
    #region ---Fields---
    [SerializeField, Required] private SceneCollectionSO nextScene;
    [SerializeField] private BossBase bossBase;
    
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

    private void Awake() {
        bossBase.OnBattleEnd += TriggerFightEndConversation;
    }
    #endregion

    private void TriggerFightEndConversation() {
        cutsceneGraph.TriggerUnityEvent("End");
        
        bossBase.OnBattleEnd -= TriggerFightEndConversation;
    }

    public void EndLevel() {
        _cameraEffects.FadeLoadScene(nextScene);
        _achievementManager.TryUnlockAchievement(IAchievementManager.AchievementID_CastleMain);
        _speedrunTimer.StopTimer();
        
    }
}