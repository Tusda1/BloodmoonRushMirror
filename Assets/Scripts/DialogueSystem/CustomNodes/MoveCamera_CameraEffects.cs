using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[UnitShortTitle("Move Camera")]
[UnitSubtitle("Moves Pixelated Render Texture. NB! set to z:-1 when done")]
[UnitCategory("DialogueUI")]
[TypeIcon(typeof(BoltAnimationEvent))]
public class MoveCamera_CameraEffects : Unit {
    #region ---Fields---
    #region ***---Control_IO---***
    [DoNotSerialize] [PortLabelHidden] public ControlInput Enter;
    [DoNotSerialize] [PortLabelHidden] public ControlOutput Exit;
    #endregion
    
    #region ***---Input---***
    public ValueInput CameraEffects;
    public ValueInput Duration;
    public ValueInput Position;
    #endregion
    #endregion
    
    protected override void Definition() {
        Enter = ControlInputCoroutine(nameof(Enter), Action);
        Exit = ControlOutput(nameof(Exit));
        Succession(Enter, Exit);
        
        CameraEffects = ValueInput<CameraEffects>(nameof(CameraEffects), null);
        Duration = ValueInput(nameof(Duration), 2f);
        Position = ValueInput(nameof(Position), new Vector3(0, 0, -1));
    }
    
    private IEnumerator Action(Flow flow) {
        var camEffects = flow.GetValue<CameraEffects>(CameraEffects);
        var duration = flow.GetValue<float>(Duration);
        var targetPosition = flow.GetValue<Vector3>(Position);
        
        var curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        var task = camEffects.MoveCamera(duration, curve, targetPosition);

        yield return new WaitUntil(() => task.IsCompleted);
        yield return Exit;
    }
}