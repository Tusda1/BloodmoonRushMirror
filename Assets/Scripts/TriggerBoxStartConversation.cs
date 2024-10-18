using Attributes;
using Unity.Behavior;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TriggerBoxStartConversation : MonoBehaviour {
    [SerializeField] private string colliderTag = "Player";
    [SerializeField, Required] private StartNewConversationEventChannel eventChannel;
    [SerializeField, Required] private BehaviorGraph conversationGraph;
    [SerializeField] private MonoBehaviour script;

    private bool _hasPlayed;

    private void OnTriggerEnter2D(Collider2D other) {
        if (colliderTag != default && !other.CompareTag(colliderTag) && !_hasPlayed) return;

        SendConversationStartEvent();
    }

    private void SendConversationStartEvent() {
        if (script != null) {
            var dialogueVariables = conversationGraph.BlackboardReference.Blackboard.Variables;
            dialogueVariables.Find(variable => variable.Type == typeof(MonoBehaviour)).ObjectValue = script;
        }

        eventChannel.SendEventMessage(conversationGraph);
        _hasPlayed = true;
    }
}