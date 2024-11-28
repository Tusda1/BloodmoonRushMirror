using Utilities.EventBus;

namespace FMODScripts {
    public class SetVolumeEvent : IEvent {
        public VolumeChanel VolumeChanel;
        public float Volume;
    }

    public enum VolumeChanel { Master, SFX, Music }
}