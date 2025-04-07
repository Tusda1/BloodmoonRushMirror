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
    #endregion

    #region ---Initialization---
    [Inject]
    public void Construct(IObjectResolver container) {
        _cameraEffects = container.Resolve<CameraEffects>();
    }
    #endregion
    
    protected override void TriggeredAction(Collider2D other) {
        cutsceneGraph.TriggerUnityEvent("Start");
    }

    public void EndLevel() {
#if DEMO_BUILD
        _cameraEffects.FadeLoadScene(endDemoSceneCollection);
#else
        _cameraEffects.FadeLoadScene(nextLevelSceneCollection);
#endif
    }
}