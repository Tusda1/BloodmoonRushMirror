using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace DialogueSystem.DialogueActions {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "SwitchMusic",
        story: "Switch Music To: [path]",
        category: "DialogueAction",
        id: "f9a0355a2faa61db84e8afde6179d44e")]
    public class SwitchMusicAction : Action {
        [SerializeReference] public BlackboardVariable<string> path;

        protected override Status OnStart() {
            FmodManager.Instance.PlayMusic(path.Value);
            return Status.Success;
        }
    }
}