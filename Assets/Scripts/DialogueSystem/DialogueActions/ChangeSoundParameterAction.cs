using System;
using UI;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Change sound parameter",
    story: "Change parameter: [parameter] to [value]",
    category: "Action",
    id: "24f1ecaec588cb75422bfd6bd0482d1e")]
public partial class ChangeSoundParameterAction : Action {
    [SerializeReference] public BlackboardVariable<string> parameter;
    [SerializeReference] public BlackboardVariable<float> value;
    
    private static DialogueUI DialogueUI => DialogueUI.Instance;

    protected override Status OnStart() {
        DialogueUI.AudioManager.SetParameterValue(parameter.Value, value.Value);
            
        return Status.Success;
    }
}