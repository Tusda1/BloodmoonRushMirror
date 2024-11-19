using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(
    fileName = nameof(ActionEventChannel),
    menuName = "Behavior/Event Channels/Create " + nameof(ActionEventChannel))]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(
    name: "ActionEventChannel",
    message: "Action",
    category: "Events",
    id: "20be8cbc1b16d58a8d5beda56a4e2937")]
public class ActionEventChannel : EventChannelBase {
    public delegate void ActionEventChannelEventHandler();

    public event ActionEventChannelEventHandler Event;

    public void SendEventMessage() {
        Event?.Invoke();
    }

    public override void SendEventMessage(BlackboardVariable[] messageData) {
        Event?.Invoke();
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback) {
        return (ActionEventChannelEventHandler)Del;

        void Del() {
            callback();
        }
    }

    public override void RegisterListener(Delegate del) {
        Event += del as ActionEventChannelEventHandler;
    }

    public override void UnregisterListener(Delegate del) {
        Event -= del as ActionEventChannelEventHandler;
    }
}