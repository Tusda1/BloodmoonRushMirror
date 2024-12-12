using Attributes;
using Unity.Behavior;
using UnityEngine;

namespace Triggers {
    public class TriggerBoxStartConversation : TriggerBox {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private BehaviorGraphAgent agent;
        
        [SerializeField, Required] private BehaviorGraph behaviorGraph;
        [SerializeField, Required] private StartNewConversationEventChannel startNewConversationEventChannel;
        #endregion

        public override void TriggeredAction(Collider2D other) {
            var dialogueVariables = agent.Graph.BlackboardReference.Blackboard.Variables;
            dialogueVariables.Find(variable => variable.Type == typeof(bool)).ObjectValue = true;
            
            // var animator = other.GetComponent<Animator>();
            //
            // Debug.Log("Triggered");
            //
            // var dialogueVariables = behaviorGraph.BlackboardReference.Blackboard.Variables;
            // dialogueVariables.Find(variable => variable.Type == typeof(Animator)).ObjectValue = animator;
            //
            // startNewConversationEventChannel.SendEventMessage(behaviorGraph);
        }
    }
}