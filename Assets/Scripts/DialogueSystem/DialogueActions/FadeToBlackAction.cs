using System;
using UI;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Fade to black",
        story: "Fade to black over [time] seconds",
        category: "DialogueAction",
        id: "83d41d0d566fd549a7d189c445b2a500")]
    internal class FadeToBlackAction : Action {
        [SerializeReference] public BlackboardVariable<float> time = new() { Value = 5.0f };

        protected override Status OnStart() {
            FadeOutController.Instance.SetAnimationTime(time.Value);
            FadeOutController.Instance.FadeOut();

            return Status.Success;
        }
    }
}