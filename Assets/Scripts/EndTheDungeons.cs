using System.Collections;
using Player;
using SceneCollections;
using Triggers;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class EndTheDungeons : TriggerBox {
    #region ---Fields---
    [SerializeField] private SceneCollectionSO endSceneCollection;
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

    public static IEnumerator PlayerWalk(float speed, float time, GravityObjectPlayerMovement playerMovement) {
        playerMovement.cutsceneMovement.x = speed;
        playerMovement.cutsceneMovementEnabled = true;
        
        yield return new WaitForSeconds(time);
        
        playerMovement.cutsceneMovement.x = 0;
        playerMovement.cutsceneMovementEnabled = false;
    }
}