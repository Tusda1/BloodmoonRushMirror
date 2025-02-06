using System;
using UI;
using Unity.Properties;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Stop Music",
        story: "Stop Music",
        category: "DialogueAction",
        id: "dfaee972cacdb7c763c853117a55acdb")]
    public class StopMusicAction : Action {
        private static DialogueUI DialogueUI => DialogueUI.Instance;
        
        protected override Status OnStart() {
            DialogueUI.AudioManager.StopAllMusic();

            return Status.Success;
        }
    }
}