using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

[UnitShortTitle("Zoom Camera")] 
[UnitCategory("DialogueUI")] 
[TypeIcon(typeof(BoltAnimationEvent))]
public class ZoomCamera : Unit {
    #region ---Fields---
    #region ***---ControlIO---***
    [DoNotSerialize] [PortLabelHidden] public ControlInput Enter;
    [DoNotSerialize] [PortLabelHidden] public ControlOutput Exit;
    #endregion
    
    #region ***---Input---***
    public ValueInput Duration;
    public ValueInput OrthographicSize;
    public ValueInput TargetOffset;
    public ValueInput ZoomAnimationCurve;
    #endregion

    private static VirtualCameraSwitch CameraSwitch => VirtualCameraSwitch.ActiveVirtualCameraSwitch;
    private static CinemachineCamera Camera => CameraSwitch.cinemachineCamera;
    #endregion
    
    protected override void Definition() {
        Enter = ControlInputCoroutine(nameof(Enter), Action);
        Exit = ControlOutput(nameof(Exit));
        Succession(Enter, Exit);
        
        Duration = ValueInput(nameof(Duration), 2f);
        OrthographicSize = ValueInput(nameof(OrthographicSize), 9f);
        TargetOffset = ValueInput(nameof(TargetOffset), Vector3.zero);
        ZoomAnimationCurve = ValueInput(nameof(ZoomAnimationCurve), AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));
    }
    
    private IEnumerator Action(Flow flow) {
        var duration = flow.GetValue<float>(Duration);
        var orthographicSize = flow.GetValue<float>(OrthographicSize);
        var targetOffset = flow.GetValue<Vector3>(TargetOffset);
        var zoomAnimationCurve = flow.GetValue<AnimationCurve>(ZoomAnimationCurve);
        
        var task = Zoom(duration, orthographicSize, targetOffset, zoomAnimationCurve);

        yield return new WaitUntil(() => task.IsCompleted);
        yield return Exit;
    }

    private static async Awaitable Zoom(float seconds, float targetSize, Vector3 targetOffset, AnimationCurve curve) {
        var startSize = Camera.Lens.OrthographicSize;
        var confiner = Camera.GetComponent<CinemachineConfiner2D>();
        
        var composer = Camera.GetComponent<CinemachinePositionComposer>();
        var startOffset = composer.TargetOffset;

        float currentProgress = 0;
        while (currentProgress < seconds) {
            currentProgress += Time.unscaledDeltaTime;
            
            var evaluatedProgress = curve.Evaluate(currentProgress / seconds);
            
            var orthographicSize = Mathf.Lerp(startSize, targetSize, evaluatedProgress);

            Camera.Lens.OrthographicSize = orthographicSize;
            confiner.OversizeWindow.MaxWindowSize = orthographicSize;
            
            composer.TargetOffset = Vector3.Lerp(startOffset, targetOffset, evaluatedProgress);
            
            await Awaitable.NextFrameAsync();
        }

        composer.TargetOffset = targetOffset;
        Camera.Lens.OrthographicSize = targetSize;
    }
}