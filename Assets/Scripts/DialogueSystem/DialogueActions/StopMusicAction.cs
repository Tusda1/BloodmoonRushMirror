using System;
using Unity.Behavior;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace DialogueSystem.DialogueActions {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "StopMusic",
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