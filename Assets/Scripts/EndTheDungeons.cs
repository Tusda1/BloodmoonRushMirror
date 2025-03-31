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


[UnitTitle("PlayerWalk")]
public class PlayerWalk : WaitUnit {
    public ValueInput playerMovement;
    public ValueInput speed;
    public ValueInput time;
    
    private GravityObjectPlayerMovement _playerMovement;
    private float _speed;
    private float _time;
    
    protected override void Definition() {
        base.Definition();
        
        playerMovement = ValueInput<GravityObjectPlayerMovement>(nameof(playerMovement), null);
        speed = ValueInput(nameof(speed), 0f);
        time = ValueInput(nameof(time), 0f);
    }

    protected override IEnumerator Await(Flow flow) {
        _playerMovement = flow.GetValue<GravityObjectPlayerMovement>(playerMovement);
        _speed = flow.GetValue<float>(speed);
        _time = flow.GetValue<float>(time);
        
        _playerMovement.cutsceneMovement.x = _speed;
        _playerMovement.cutsceneMovementEnabled = true;
        
        yield return new WaitForSeconds(_time);
        
        _playerMovement.cutsceneMovement.x = 0;
        _playerMovement.cutsceneMovementEnabled = false;
        
        yield return exit;
    }
}
