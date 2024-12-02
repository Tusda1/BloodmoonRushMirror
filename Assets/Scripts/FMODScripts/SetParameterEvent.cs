using Utilities.EventBus;

namespace FMODScripts {
    public struct SetParameterEvent : IEvent {
        public readonly string Parameter;
        public readonly float Value;
        public readonly bool IgnoreSeekSpeed;

        public SetParameterEvent(string parameter, float value, bool ignoreSeekSpeed) {
            Parameter = parameter;
            Value = value;
            IgnoreSeekSpeed = ignoreSeekSpeed;
        }
    }
}