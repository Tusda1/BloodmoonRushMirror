using UnityEngine;
using Utilities.EventBus;

namespace FMODScripts {
    public struct SetVolumeEvent : IEvent {
        public readonly VolumeChanel VolumeChanel;
        public readonly float Volume;

        public SetVolumeEvent(VolumeChanel volumeChanel, float volume) {
            VolumeChanel = volumeChanel;
            Volume = Mathf.Clamp01(volume);
        }
    }

    public enum VolumeChanel { Master, SFX, Music }
}