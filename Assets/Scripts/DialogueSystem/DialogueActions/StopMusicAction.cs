using System;
using FMODScripts;
using Unity.Properties;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Stop Music",
        story: "Stop Music",
        category: "DialogueAction",
        id: "dfaee972cacdb7c763c853117a55acdb")]
    public class StopMusicAction : Action {
        private static FMODManager FMODManager => FMODManager.Instance;
        
        protected override Status OnStart() {
            FMODManager.StopMusic();

            return Status.Success;
        }
    }
}