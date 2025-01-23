using System;
using System.Collections;
using Attributes;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Utilities.EventBus;

namespace FMODScripts {
    public class FMODManager : Singleton<FMODManager> {
        #region ---Fields---
        [Header("DebugLogger")] [SerializeField, Required]
        private DebugLoggerSO debugLogger;

        public static float MasterVolume {
            get {
                if (PlayerPrefs.HasKey("MasterVolume")) return PlayerPrefs.GetFloat("MasterVolume");
                
                PlayerPrefs.SetFloat("MasterVolume", 1);
                return 1;
            }
        }

        public static float MusicVolume {
            get {
                if (PlayerPrefs.HasKey("SFXVolume")) return PlayerPrefs.GetFloat("SFXVolume");
                
                PlayerPrefs.SetFloat("SFXVolume", 1);
                return 1;
            }
        }

        public static float SfxVolume {
            get {
                if (PlayerPrefs.HasKey("MusicVolume")) return PlayerPrefs.GetFloat("MusicVolume");
                
                PlayerPrefs.SetFloat("MusicVolume", 1);
                return 1;
            }
        }

        private EventInstance _currentMusic;
        private Bus _musicBus;
        private Bus _sfxBus;
        private Bus _masterBus;

        private static string BusPath => "bus:/";
        public static string SFXPath => "event:/Sfx/";
        public static string MusicPath => "event:/Music/";

        private EventBinding<IFMODEvent> _eventBinding;
        #endregion

        #region ---EventFunctions---
        private IEnumerator Start() {
            debugLogger.Log("Initializing");

            yield return new WaitUntil(() => RuntimeManager.IsInitialized);
            debugLogger.Log("RuntimeManager is initialized");

            _masterBus = RuntimeManager.GetBus(BusPath);
            _musicBus = RuntimeManager.GetBus(BusPath + "Music Bus");
            _sfxBus = RuntimeManager.GetBus(BusPath + "Sfx Bus");

            if (_masterBus.Equals(default) || _sfxBus.Equals(default) || _musicBus.Equals(default)) {
                debugLogger.Log("Busses are not valid");
            }
            
            SetMasterVolume(MasterVolume);
            SetMusicVolume(MusicVolume);
            SetSfxVolume(SfxVolume);
        }
        #endregion

        #region ---EventBus---
        private void OnEnable() {
            _eventBinding = new EventBinding<IFMODEvent>(HandleEvent);

            EventBus<IFMODEvent>.Register(_eventBinding);
        }

        private void OnDisable() {
            EventBus<IFMODEvent>.Deregister(_eventBinding);
        }

        private void HandleEvent(IFMODEvent fmodEvent) {
            switch (fmodEvent) {
                case PlaySoundEvent playSoundEvent:
                    HandlePlaySoundEvent(playSoundEvent);
                    break;
                case SetParameterEvent setParameterEvent:
                    HandleSetParameterEvent(setParameterEvent);
                    break;
                case SetVolumeEvent setVolumeEvent:
                    HandleSetVolumeEvent(setVolumeEvent);
                    break;
                case StopSoundEvent stopSoundEvent:
                    HandleStopSoundEvent(stopSoundEvent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fmodEvent));
            }
        }

        #region ***---HandleEvents---***
        private void HandleSetVolumeEvent(SetVolumeEvent setVolumeEvent) {
            switch (setVolumeEvent.VolumeChanel) {
                case VolumeChanel.Master:
                    SetMasterVolume(setVolumeEvent.Volume);
                    break;
                case VolumeChanel.SFX:
                    SetSfxVolume(setVolumeEvent.Volume);
                    break;
                case VolumeChanel.Music:
                    SetMusicVolume(setVolumeEvent.Volume);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void HandlePlaySoundEvent(PlaySoundEvent playSoundEvent) {
            switch (playSoundEvent.SoundType) {
                case SoundType.None:
                case SoundType.SFX:
                    if (playSoundEvent.SoundObject != null) {
                        PlayOneShotAttached(playSoundEvent.SoundPath, playSoundEvent.SoundObject);
                    }
                    else {
                        PlayOneShot(playSoundEvent.SoundPath);
                    }

                    break;
                case SoundType.Music:
                    PlayMusic(playSoundEvent.SoundPath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void HandleSetParameterEvent(SetParameterEvent setParameterEvent) {
            SetParameterValue(setParameterEvent.Parameter, setParameterEvent.Value, setParameterEvent.IgnoreSeekSpeed);
        }
        
        private void HandleStopSoundEvent(StopSoundEvent stopSoundEvent) {
            switch (stopSoundEvent.SoundType) {
                case StopSoundType.Music:
                    StopMusic();
                    break;
                case StopSoundType.All:
                    StopAllSound();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion

        #region ***---RaiseEvents---***
        public static void RaiseEvent(IFMODEvent fmodEvent) {
            EventBus<IFMODEvent>.Raise(fmodEvent);
        }
        
        public static void RaiseSetVolumeEvent(VolumeChanel volumeChanel, float volume) {
            EventBus<IFMODEvent>.Raise(new SetVolumeEvent(volumeChanel, volume));
        }

        public static void RaiseSFXEvent(string soundName, GameObject soundObject = null) {
            EventBus<IFMODEvent>.Raise(new PlaySoundEvent(SoundType.SFX, soundName, soundObject));
        }

        public static void RaiseMusicEvent(string soundName, GameObject soundObject = null) {
            EventBus<IFMODEvent>.Raise(new PlaySoundEvent(SoundType.Music, soundName, soundObject));
        }

        public static void RaiseMusicEvent(string soundName, string parameter, float value,
            bool ignoreSeekSpeed = false) {
            if (Instance == null || !Instance.IsMusicPlaying()) {
                RaiseMusicEvent(soundName);
            }

            RaiseSetParameterEvent(parameter, value, ignoreSeekSpeed);
        }

        public static void RaiseSetParameterEvent(string parameter, float value, bool ignoreSeekSpeed = false) {
            EventBus<IFMODEvent>.Raise(new SetParameterEvent(parameter, value, ignoreSeekSpeed));
        }

        public static void RaiseStopSoundEvent(StopSoundType soundType) {
            EventBus<IFMODEvent>.Raise(new StopSoundEvent(soundType));
        }
        #endregion
        #endregion

        #region ---SetVolume---
        private void SetMasterVolume(float volume) {
            _masterBus.setVolume(volume);
            PlayerPrefs.SetFloat("MasterVolume", volume);

            debugLogger.Log("Master volume set to: " + volume);
        }

        private void SetSfxVolume(float volume) {
            _sfxBus.setVolume(volume);
            PlayerPrefs.SetFloat("SFXVolume", volume);

            debugLogger.Log("SFX volume set to: " + volume);
        }

        private void SetMusicVolume(float volume) {
            _musicBus.setVolume(volume);
            PlayerPrefs.SetFloat("MusicVolume", volume);

            debugLogger.Log("Music volume set to: " + volume);
        }
        #endregion

        #region ---PlaySound---
        private void PlayOneShotAttached(string path, GameObject emitter) {
            if (CheckIfStringIsEmpty(path)) return;

            RuntimeManager.PlayOneShotAttached(path, emitter);

            debugLogger.Log($"Playing one shot attached to {emitter.name}");
        }

        private void PlayOneShot(string path) {
            if (CheckIfStringIsEmpty(path)) return;

            RuntimeManager.PlayOneShot(path);

            debugLogger.Log("Playing one shot global");
        }

        private void PlayMusic(string path) {
            if (CheckIfStringIsEmpty(path)) return;

            _currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _currentMusic = RuntimeManager.CreateInstance(path);
            _currentMusic.start();

            debugLogger.Log("Playing new music");
        }
        #endregion

        #region ---SetParameter---
        private void SetParameterValue(string parameter, float value, bool ignoreSeekSpeed = false) {
            if (CheckIfStringIsEmpty(parameter)) return;

            _currentMusic.setParameterByName(parameter, value, ignoreSeekSpeed);

            debugLogger.Log($"Parameter {parameter} set to {value}");
        }
        #endregion

        #region ---StopSound---
        private void StopMusic() {
            _currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            debugLogger.Log("Stopped music");
        }

        private void StopAllSound() {
            _masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            debugLogger.Log("Stopped all sound");
        }
        #endregion

        private bool IsMusicPlaying() {
            return _currentMusic.GetPlaybackState() == PLAYBACK_STATE.PLAYING;
        }

        private bool CheckIfStringIsEmpty(string s) {
            if (!string.IsNullOrWhiteSpace(s)) return false;
            Debug.LogWarning($"[{debugLogger.logPrefix}] String received is null or empty");
            return true;
        }
    }
}