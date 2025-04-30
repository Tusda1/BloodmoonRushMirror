using SceneCollections;
using Triggers;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class EndTheDungeons : TriggerBox {
    #region ---Fields---
    [SerializeField] private SceneCollectionSO nextLevelSceneCollection;
    [SerializeField] private SceneCollectionSO endDemoSceneCollection;
    [SerializeField] private ScriptMachine cutsceneGraph;

    private CameraEffects _cameraEffects;
    private IAchievementManager _achievementManager;
    private ISpeedrunTimer _speedrunTimer;
    #endregion

    #region ---Initialization---
    [Inject]
    public void Construct(IObjectResolver container) {
        _cameraEffects = container.Resolve<CameraEffects>();
        _achievementManager = container.Resolve<IAchievementManager>();
        _speedrunTimer = container.Resolve<ISpeedrunTimer>();
    }
    #endregion
    
    protected override void TriggeredAction(Collider2D other) {
#if DEMO_BUILD
        cutsceneGraph.TriggerUnityEvent("Demo_Start");
#else
        cutsceneGraph.TriggerUnityEvent("Start");
#endif
    }

    public void EndLevel() {
#if DEMO_BUILD
        _cameraEffects.FadeLoadScene(endDemoSceneCollection);
#else
        _cameraEffects.FadeLoadScene(nextLevelSceneCollection);
        _achievementManager.TryUnlockAchievement(IAchievementManager.AchievementID_TheDungeons);
        _speedrunTimer.StopTimer();
#endif
    }
}