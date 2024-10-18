using System;
using UI;
using Unity.Behavior;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine;

namespace DialogueSystem.DialogueActions {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "FadeToBlack",
        story: "Fade To Black Over [time] Seconds",
        category: "DialogueAction",
        id: "83d41d0d566fd549a7d189c445b2a500")]
    public class FadeToBlackAction : Action {
        [SerializeReference] public BlackboardVariable<float> time = new() { Value = 5.0f };

        protected override Status OnStart() {
            FadeOutController.Instance.SetAnimationTime(time.Value);
            FadeOutController.Instance.FadeOut();

            return Status.Success;
        }
    }
}