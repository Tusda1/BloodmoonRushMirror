using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using ExtensionMethods;

namespace DialogueSystem.DialogueActions {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WaitUntilAnimationIsPlaying",
        story: "Wait Until [animationClip] In [animator] Is Playing Equals: [value]",
        category: "DialogueAction",
        id: "f1bd2467a51526bd2150986cc97eba40")]
    public class WaitUntilAnimationIsPlayingAction : Action {
        [SerializeReference] public BlackboardVariable<AnimationClip> animationClip;
        [SerializeReference] public BlackboardVariable<Animator> animator;
        [SerializeReference] public BlackboardVariable<bool> value;

        protected override Status OnUpdate() {
            return animator.Value.GetCurrentAnimationClip() == animationClip.Value == value.Value
                ? Status.Success
                : Status.Running;
        }
    }
}