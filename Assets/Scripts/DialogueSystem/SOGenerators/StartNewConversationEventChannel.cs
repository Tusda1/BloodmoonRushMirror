using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/StartNewConversationEventChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(
    name: "StartNewConversationEventChannel",
    message: "Start [SubGraph] Event",
    category: "Events",
    id: "afb2aecc5c73e5e1e8aded6e1624f8f1")]
public class StartNewConversationEventChannel : EventChannelBase {
    public delegate void StartNewConversationEventChannelEventHandler(BehaviorGraph SubGraph);

    public event StartNewConversationEventChannelEventHandler Event;

    public void SendEventMessage(BehaviorGraph SubGraph) {
        Event?.Invoke(SubGraph);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData) {
        BlackboardVariable<BehaviorGraph> SubGraphBlackboardVariable =
            messageData[0] as BlackboardVariable<BehaviorGraph>;
        var SubGraph = SubGraphBlackboardVariable != null ? SubGraphBlackboardVariable.Value : default(BehaviorGraph);

        Event?.Invoke(SubGraph);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback) {
        StartNewConversationEventChannelEventHandler del = (SubGraph) => {
            BlackboardVariable<BehaviorGraph> var0 = vars[0] as BlackboardVariable<BehaviorGraph>;
            if (var0 != null)
                var0.Value = SubGraph;

            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del) {
        Event += del as StartNewConversationEventChannelEventHandler;
    }

    public override void UnregisterListener(Delegate del) {
        Event -= del as StartNewConversationEventChannelEventHandler;
    }
}