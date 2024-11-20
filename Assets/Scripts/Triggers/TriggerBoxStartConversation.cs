using Attributes;
using Unity.Behavior;

namespace Triggers {
    public class TriggerBoxStartConversation : TriggerBox {
        #region ---Fields---
        [GetOrAddComponent] private BehaviorGraphAgent _agent;
        #endregion

        public override void TriggeredAction() {
            _agent.enabled = true;
        }
    }
}