using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace DialogueSystem.DialogueActions {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "PlaySFX",
        story: "Play SFX: [sfxName]",
        category: "DialogueAction",
        id: "a7c30756d907cc458e2475160bdbb3ef")]
    public class PlaySfxAction : Action {
        [SerializeReference] public BlackboardVariable<string> sfxName;

        protected override Status OnStart() {
            FmodManager.Instance.PlayOneShot(sfxName.Value);

            return Status.Success;
        }
    }
}