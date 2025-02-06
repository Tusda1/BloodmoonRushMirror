using System;
using UI;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Change Music",
        story: "Change music to: [musicID]",
        category: "DialogueAction",
        id: "f9a0355a2faa61db84e8afde6179d44e")]
    public class ChangeMusicAction : Action {
        [SerializeReference] public BlackboardVariable<string> musicID;
        private static DialogueUI DialogueUI => DialogueUI.Instance;

        protected override Status OnStart() {
            DialogueUI.AudioManager.PlayMusic(musicID.Value);
            
            return Status.Success;
        }
    }
}