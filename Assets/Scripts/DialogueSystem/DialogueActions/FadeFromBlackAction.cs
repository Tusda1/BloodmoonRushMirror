using System;
using UI;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace DialogueSystem.DialogueActions {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "FadeFromBlack",
        story: "Fade From Black Over [time] Seconds",
        category: "Action",
        id: "490e94aa3754ce7782fc5a9dd6ec504a")]
    public class FadeFromBlackAction : Action {
        [SerializeReference] public BlackboardVariable<float> time = new() { Value = 5.0f };

        protected override Status OnStart() {
            FadeOutController.Instance.SetAnimationTime(time.Value);
            FadeOutController.Instance.FadeIn();

            return Status.Success;
        }
    }
}