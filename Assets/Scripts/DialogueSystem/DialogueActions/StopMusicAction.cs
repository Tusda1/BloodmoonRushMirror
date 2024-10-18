using System;
using Unity.Properties;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Stop Music",
        story: "Stop Music",
        category: "DialogueAction",
        id: "dfaee972cacdb7c763c853117a55acdb")]
    public class StopMusicAction : Action {
        protected override Status OnStart() {
            FmodManager.Instance.StopMusic();

            return Status.Success;
        }
    }
}