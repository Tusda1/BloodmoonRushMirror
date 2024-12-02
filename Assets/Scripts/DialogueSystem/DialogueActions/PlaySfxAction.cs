using System;
using FMODScripts;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Play SFX",
        story: "Play SoundEvent: [sfxName] | Optional path prefix [soundType]",
        category: "DialogueAction",
        id: "a7c30756d907cc458e2475160bdbb3ef")]
    public class PlaySfxAction : Action {
        [SerializeReference] public BlackboardVariable<string> sfxName;
        [SerializeReference] public BlackboardVariable<SoundType> soundType;

        protected override Status OnStart() {
            FMODManager.RaiseEvent(new PlaySoundEvent(soundType.Value, sfxName.Value));

            return Status.Success;
        }
    }
}