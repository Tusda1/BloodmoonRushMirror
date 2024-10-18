using System;
using UI;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Fade From Black",
        story: "Fade from black over [time] seconds",
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