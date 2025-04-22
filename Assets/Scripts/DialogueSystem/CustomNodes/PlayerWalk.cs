using System.Collections;
using Player;
using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("PlayerWalk")]
[UnitCategory("DialogueUI")]
public class PlayerWalk : WaitUnit {
    public ValueInput PlayerMovement;
    public ValueInput WalkRight;
    public ValueInput Speed;
    public ValueInput Time;

    protected override void Definition() {
        base.Definition();

        PlayerMovement = ValueInput<GravityObjectPlayerMovement>(nameof(PlayerMovement), null);
        WalkRight = ValueInput(nameof(WalkRight), true);
        Speed = ValueInput(nameof(Speed), 0f);
        Time = ValueInput(nameof(Time), 0f);
    }

    protected override IEnumerator Await(Flow flow) {
        var playerMovement = flow.GetValue<GravityObjectPlayerMovement>(PlayerMovement);
        var walkRight = flow.GetValue<bool>(WalkRight);
        var speed = flow.GetValue<float>(Speed) * (walkRight ? 1 : -1);
        var time = flow.GetValue<float>(Time);

        playerMovement.cutsceneMovement.x = speed;
        playerMovement.cutsceneMovementEnabled = true;

        yield return new WaitForSeconds(time);

        playerMovement.cutsceneMovement.x = 0;
        playerMovement.cutsceneMovementEnabled = false;

        yield return exit;
    }
}