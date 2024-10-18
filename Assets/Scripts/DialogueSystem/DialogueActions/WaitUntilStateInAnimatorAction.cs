using System;
using ExtensionMethods;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait Until State In Animator",
        story: "Wait until [animator] state's name is [name] equals [value]",
        category: "DialogueAction",
        id: "be691507f32d26bcda4fdcacce2ff67b")]
    public class WaitUntilStateInAnimatorAction : Action {
        [SerializeReference] public BlackboardVariable<Animator> animator;
        [SerializeReference] public BlackboardVariable<string> name;
        [SerializeReference] public BlackboardVariable<bool> value = new(true);

        protected override Status OnUpdate() {
            return animator.Value.GetCurrentAnimationClip().name == name == value.Value
                ? Status.Success
                : Status.Running;
        }
    }
}