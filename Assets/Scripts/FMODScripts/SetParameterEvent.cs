using Utilities.EventBus;

namespace FMODScripts {
    public class SetParameterEvent : IEvent {
        public string Parameter;
        public float Value;
        public bool IgnoreSeekSpeed;
    }
}