using System;
using UI;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace DialogueSystem.DialogueActions {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "SetInConversation",
        story: "In Conversation Is [value]",
        category: "DialogueAction",
        id: "2d3263a2852965ac7cd5fc6148adb85f")]
    public class SetInConversationAction : Action {
        [SerializeReference] public BlackboardVariable<bool> value;

        protected override Status OnStart() {
            if (DialogueUI.Instance == null) return Status.Failure;

            if (value.Value == false) {
                DialogueUI.Instance.uiDocumentVisible = value.Value;
            }

            DialogueUI.Instance.dialogueText = "";

            DialogueUI.Instance.playerCanMove.ChangeVote(DialogueUI.Instance.scriptVotePair, !value.Value);
            return Status.Success;
        }
    }
}