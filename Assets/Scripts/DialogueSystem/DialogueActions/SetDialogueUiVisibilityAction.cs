using System;
using UI;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set DialogueUI Visibility",
        story: "Set dialogueUI visibility to [value]",
        category: "Action",
        id: "875940953991b078f36e684d6c237500")]
    public class SetDialogueUiVisibilityAction : Action {
        [SerializeReference] public BlackboardVariable<bool> value;

        private DialogueUI DialogueUI => DialogueUI.Instance;

        protected override Status OnStart() {
            if (DialogueUI == null) return Status.Failure;

            DialogueUI.uiDocumentVisible = value.Value;
            return Status.Success;
        }
    }
}