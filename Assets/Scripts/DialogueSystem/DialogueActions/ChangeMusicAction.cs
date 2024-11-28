using System;
using FMODScripts;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Change Music",
        story: "Change music to: [path]",
        category: "DialogueAction",
        id: "f9a0355a2faa61db84e8afde6179d44e")]
    public class ChangeMusicAction : Action {
        [SerializeReference] public BlackboardVariable<string> path;

        protected override Status OnStart() {
            FMODManager.RaiseMusicEvent(path.Value);
            
            return Status.Success;
        }
    }
}