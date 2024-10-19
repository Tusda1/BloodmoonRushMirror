using System;
using Fmod;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Play SFX",
        story: "Play SFX: [sfxName]",
        category: "DialogueAction",
        id: "a7c30756d907cc458e2475160bdbb3ef")]
    public class PlaySfxAction : Action {
        [SerializeReference] public BlackboardVariable<string> sfxName;
        
        private FmodManager fmodManager => FmodManager.Instance;

        protected override Status OnStart() {
            fmodManager.PlayOneShot(sfxName.Value);

            return Status.Success;
        }
    }
}