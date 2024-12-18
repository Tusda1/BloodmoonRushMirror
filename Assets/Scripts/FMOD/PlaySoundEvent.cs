using System;
using UnityEngine;

namespace FMODScripts {
    public struct PlaySoundEvent : IFMODEvent {
        public readonly string SoundPath;
        public SoundType SoundType;
        public readonly GameObject SoundObject;

        public PlaySoundEvent(string soundPath, GameObject soundObject = null) {
            SoundPath = soundPath;
            SoundType = SoundType.None;
            SoundObject = soundObject;
        }

        public PlaySoundEvent(SoundType soundType, string soundPath, GameObject soundObject = null) {
            SoundType = soundType;
            SoundPath = SoundType switch {
                SoundType.None => soundPath,
                SoundType.SFX => FMODManager.SFXPath + soundPath,
                SoundType.Music => FMODManager.MusicPath + soundPath,
                _ => throw new ArgumentOutOfRangeException(nameof(SoundType), SoundType, null)
            };
            SoundObject = soundObject;
        }
    }

    [Serializable] public enum SoundType { None, SFX, Music }
}