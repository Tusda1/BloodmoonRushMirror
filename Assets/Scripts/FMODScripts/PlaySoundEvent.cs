using System;
using UnityEngine;
using Utilities.EventBus;

namespace FMODScripts {
    public class PlaySoundEvent : IEvent {
        public SoundType SoundType;
        public GameObject SoundObject;
        
        public string SoundPath {
            get {
                return SoundType switch {
                    SoundType.SFX => FMODManager.SFXPath + _soundPath,
                    SoundType.Music => FMODManager.MusicPath + _soundPath,
                    _ => throw new ArgumentOutOfRangeException(nameof(SoundType), SoundType, null)
                };
            }
            
            set => _soundPath = value;
        }
        
        private string _soundPath;
    }

    public enum SoundType { SFX, Music }
}