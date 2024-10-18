using System;
using UI;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace DialogueSystem.DialogueActions {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "SetDialogueUIVisibility",
        story: "Set DialogueUI Visibility To [value]",
        category: "DialogueAction",
        id: "1d559a13de6996119c80648c6ad1dfcd")]
    public class SetDialogueUiVisibilityAction : Action {
        [SerializeReference] public BlackboardVariable<bool> value;

        protected override Status OnStart() {
            if (DialogueUI.Instance == null) return Status.Failure;

            DialogueUI.Instance.uiDocumentVisible = value.Value;
            return Status.Success;
        }
    }
}