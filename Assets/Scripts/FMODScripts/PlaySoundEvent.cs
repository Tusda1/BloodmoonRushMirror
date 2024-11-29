using System;
using UnityEngine;
using Utilities.EventBus;

namespace FMODScripts {
    public struct PlaySoundEvent : IEvent {
        public SoundType SoundType;
        public readonly GameObject SoundObject;

        public readonly string SoundPath;

        public PlaySoundEvent(SoundType soundType, string soundPath, GameObject soundObject = null) {
            SoundType = soundType;
            SoundPath = SoundType switch {
                SoundType.SFX => FMODManager.SFXPath + soundPath,
                SoundType.Music => FMODManager.MusicPath + soundPath,
                _ => throw new ArgumentOutOfRangeException(nameof(SoundType), SoundType, null)
            };
            SoundObject = soundObject;
        }
    }

    public enum SoundType { SFX, Music }
}