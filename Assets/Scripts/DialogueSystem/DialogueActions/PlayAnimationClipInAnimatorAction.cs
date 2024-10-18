using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace DialogueSystem.DialogueActions {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "PlayAnimationClipInAnimator",
        story: "Play [animationClip] In [animator]",
        category: "DialogueAction",
        id: "4b6e15fa71454678d43163605c5687ba")]
    public class PlayAnimationClipInAnimatorAction : Action {
        [SerializeReference] public BlackboardVariable<AnimationClip> animationClip;
        [SerializeReference] public BlackboardVariable<Animator> animator;

        protected override Status OnStart() {
            animator.Value.Play(animationClip.Value.name);

            return Status.Success;
        }
    }
}