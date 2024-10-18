using System;
using Fmod;
using Unity.Properties;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Stop Music",
        story: "Stop Music",
        category: "DialogueAction",
        id: "dfaee972cacdb7c763c853117a55acdb")]
    public class StopMusicAction : Action {
        private FmodManager fmodManager => FmodManager.Instance;
        
        protected override Status OnStart() {
            fmodManager.StopMusic();

            return Status.Success;
        }
    }
}