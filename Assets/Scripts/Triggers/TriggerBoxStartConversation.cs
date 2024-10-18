using Attributes;
using Unity.Behavior;
using UnityEngine;

namespace Triggers {
    public class TriggerBoxStartConversation : TriggerBox {
        [SerializeField, Required] private StartNewConversationEventChannel eventChannel;
        [SerializeField, Required] private BehaviorGraph conversationGraph;
        [SerializeField] private MonoBehaviour script;

        public override void TriggeredAction() {
            if (script != null) {
                var dialogueVariables = conversationGraph.BlackboardReference.Blackboard.Variables;
                dialogueVariables.Find(variable => variable.Type == typeof(MonoBehaviour)).ObjectValue = script;
            }

            eventChannel.SendEventMessage(conversationGraph);
        }
    }
}