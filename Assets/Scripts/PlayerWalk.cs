using System.Collections;
using Player;
using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("PlayerWalk")]
[UnitCategory("DialogueUI")] 
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