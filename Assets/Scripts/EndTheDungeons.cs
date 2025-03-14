using SceneCollections;
using Triggers;
using UnityEngine;
using VContainer;

public class EndTheDungeons : TriggerBox {
    #region ---Fields---
    [SerializeField] private SceneCollectionSO endSceneCollection;
    
    [Inject] private CameraEffects _cameraEffects;
    #endregion
    
    protected override void TriggeredAction(Collider2D other) {
        _cameraEffects.FadeLoadScene(endSceneCollection);
    }
}