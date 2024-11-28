using Utilities.EventBus;

namespace FMODScripts {
    public class StopSoundEvent : IEvent {
        public StopSoundType SoundType;
    }

    public enum StopSoundType { Music, All }
}