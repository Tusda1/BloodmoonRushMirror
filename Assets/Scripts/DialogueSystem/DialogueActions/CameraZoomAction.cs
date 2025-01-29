using System;
using Unity.Behavior;
using Unity.Cinemachine;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "CameraZoom", 
    story: "Zoom Camera To [size] Over [duration] Seconds | TimeSlow: [slowTime]", 
    category: "Action",
    id: "5a9f0241d0a338000cc285b297afde42")]
public partial class CameraZoomAction : Action {
    [SerializeReference] public BlackboardVariable<float> size;
    [SerializeReference] public BlackboardVariable<float> duration;
    [SerializeReference] public BlackboardVariable<bool> slowTime;

    private VirtualCameraSwitch CameraSwitch => VirtualCameraSwitch.ActiveVirtualCameraSwitch;
    private CinemachineCamera Camera => CameraSwitch.cinemachineCamera;
    
    private Awaitable _zoomTask;

    protected override Status OnStart() {
        _zoomTask = Zoom(duration, size);

        if (slowTime) {
            _ = CameraSwitch._cameraEffects.TimeSlow(duration, CameraSwitch.timeSlowCurve);
        }
        
        return Status.Running;
    }

    protected override Status OnUpdate() {
        return _zoomTask.IsCompleted ? Status.Success : Status.Running;
    }

    protected override void OnEnd() {
    }


    public async Awaitable Zoom(float seconds, float targetSize, AnimationCurve curve = null) {
        curve ??= AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        var startSize = Camera.Lens.OrthographicSize;
        var confiner = Camera.GetComponent<CinemachineConfiner2D>();

        float currentProgress = 0;
        while (currentProgress < seconds) {
            currentProgress += Time.unscaledDeltaTime;
            
            var orthographicSize = Mathf.Lerp(startSize, targetSize, curve.Evaluate(currentProgress));

            Camera.Lens.OrthographicSize = orthographicSize;
            confiner.OversizeWindow.MaxWindowSize = orthographicSize;
            
            await Awaitable.NextFrameAsync();
        }

        Camera.Lens.OrthographicSize = targetSize;
    }
}