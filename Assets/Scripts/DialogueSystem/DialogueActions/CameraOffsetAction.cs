using System;
using UI;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "CameraOffset", 
    story: "Move RenderTextureCamera To: [vector3] Over [duration] Seconds", 
    category: "Action", 
    id: "a0a6bbd9da519a678d1764ab89505147")]
public partial class CameraOffsetAction : Action {
    [SerializeReference] public BlackboardVariable<Vector3> vector3;
    [SerializeReference] public BlackboardVariable<float> duration;
    
    private DialogueUI DialogueUI => DialogueUI.Instance;
    
    private bool _taskCompleted;

    protected override Status OnStart() {
        _taskCompleted = false;
        
        DialogueUI.CameraEffects.TriggerMoveCamera(duration.Value, vector3.Value, () => { _taskCompleted = true; });
        return Status.Running;
    }

    protected override Status OnUpdate() {
        if (_taskCompleted) {
            return Status.Success;
        }
        
        return Status.Running;
    }
}

