using System;
using UI;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior {
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set In Conversation",
        story: "Set in conversation to [value]",
        category: "Action",
        id: "2c0139d7eada6337995f99f22e9bd8b7")]
    public class SetInConversationAction : Action {
        [SerializeReference] public BlackboardVariable<bool> value;

        private DialogueUI dialogueUI => DialogueUI.Instance;

        protected override Status OnStart() {
            if (dialogueUI == null) return Status.Failure;

            if (value.Value == false) {
                dialogueUI.uiDocumentVisible = value.Value;
            }

            dialogueUI.dialogueText = "";

            dialogueUI.playerCanMove.ChangeVote(dialogueUI.scriptVotePair, !value.Value);
            return Status.Success;
        }
    }
}