using Attributes;
using SceneCollections;
using UnityEngine;
using VContainer;

public class DraculaFight : ConversationTrigger {
    #region ---Fields---
    [SerializeField, Required] private SceneCollectionSO nextScene;
    [SerializeField] private BossBase bossBase;
    
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

    private void Awake() {
        bossBase.OnBattleEnd += TriggerFightEndConversation;
        bossBase.OnBattlePause += TriggerFightBreakConversation;
    }
    #endregion

    public void End() {
        _cameraEffects.FadeLoadScene(nextScene);
        _achievementManager.TryUnlockAchievement(IAchievementManager.AchievementID_DraculasOffice);
    }

    private void TriggerFightBreakConversation() {
        cutsceneGraph.TriggerUnityEvent("FightBreak");
    }

    private void TriggerFightEndConversation() {
        cutsceneGraph.TriggerUnityEvent("FightEnd");
    }
}