using System;
using UI;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Play SFX", 
        story: "Play SFX: [sfxID]", 
        category: "DialogueAction", 
        id: "a7c30756d907cc458e2475160bdbb3ef")]
    public class PlaySfxAction : Action {
        [SerializeReference] public BlackboardVariable<string> sfxID;
        private static DialogueUI DialogueUI => DialogueUI.Instance;

        protected override Status OnStart() {
            DialogueUI.AudioManager.PlayOneShot(sfxID.Value);

            return Status.Success;
        }
    }
}