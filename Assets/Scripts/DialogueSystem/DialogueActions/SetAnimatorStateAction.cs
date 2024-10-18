using System;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set Animator State",
        story: "Set [animator] state to [name]",
        category: "Action/Animation",
        id: "4b6e15fa71454678d43163605c5687ba")]
    public class SetAnimatorStateAction : Action {
        [SerializeReference] public BlackboardVariable<Animator> animator;
        [SerializeReference] public BlackboardVariable<string> name;

        protected override Status OnStart() {
            animator.Value.Play(name.Value);

            return Node.Status.Success;
        }
    }
}