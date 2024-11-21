using Attributes;
using Unity.Behavior;
using UnityEngine;

namespace Triggers {
    public class TriggerBoxStartConversation : TriggerBox {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private BehaviorGraphAgent agent;
        #endregion

        public override void TriggeredAction() {
            agent.enabled = true;
        }
    }
}