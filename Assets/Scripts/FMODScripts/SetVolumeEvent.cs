using UnityEngine;

namespace FMODScripts {
    public struct SetVolumeEvent : IFMODEvent {
        public readonly VolumeChanel VolumeChanel;
        public readonly float Volume;

        public SetVolumeEvent(VolumeChanel volumeChanel, float volume) {
            VolumeChanel = volumeChanel;
            Volume = Mathf.Clamp01(volume);
        }
    }

    public enum VolumeChanel { Master, SFX, Music }
}