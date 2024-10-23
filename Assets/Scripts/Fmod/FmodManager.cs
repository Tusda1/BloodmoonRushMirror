using System.Collections;
using System.Runtime.InteropServices;
using Attributes;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Fmod {
    public class FmodManager : PersistentSingleton<FmodManager> {
        [Header("DebugLogger")] [SerializeField, Required]
        private DebugLoggerSO debugLogger;

        public static float masterVolume => PlayerPrefs.GetFloat("MasterVolume");
        public static float musicVolume => PlayerPrefs.GetFloat("SFXVolume");
        public static float sfxVolume => PlayerPrefs.GetFloat("MusicVolume");

        private EventInstance _currentMusic;
        private Bus _musicBus;
        private Bus _sfxBus;
        private Bus _masterBus;

        private static string busPath => "bus:/";
        private static string sfxPath => "event:/Sfx/";
        private static string musicPath => "event:/Music/";

        private IEnumerator Start() {
            debugLogger.Log("Initializing");

            yield return new WaitUntil(() => RuntimeManager.IsInitialized);
            debugLogger.Log("RuntimeManager is initialized");

            _masterBus = RuntimeManager.GetBus(busPath);
            _musicBus = RuntimeManager.GetBus(busPath + "Music Bus");
            _sfxBus = RuntimeManager.GetBus(busPath + "Sfx Bus");

            if (_masterBus.Equals(default) || _sfxBus.Equals(default) || _musicBus.Equals(default)) {
                debugLogger.Log("Busses are not valid");
            }
        }

        #region ---PublicFunctions---
        public void SetMasterVolume(float volume) {
            volume = Mathf.Clamp01(volume);

            _masterBus.setVolume(volume);
            PlayerPrefs.SetFloat("MasterVolume", volume);

            debugLogger.Log("Master volume set to: " + volume);
        }

        public void SetSfxVolume(float volume) {
            volume = Mathf.Clamp01(volume);

            _sfxBus.setVolume(volume);
            PlayerPrefs.SetFloat("SFXVolume", volume);

            debugLogger.Log("SFX volume set to: " + volume);
        }

        public void SetMusicVolume(float volume) {
            volume = Mathf.Clamp01(volume);

            _musicBus.setVolume(volume);
            PlayerPrefs.SetFloat("MusicVolume", volume);

            debugLogger.Log("Music volume set to: " + volume);
        }

        public void PlayOneShotAttached(string path, GameObject emitter) {
            if (CheckIfStringIsEmpty(path)) return;

            RuntimeManager.PlayOneShotAttached(path, emitter);

            debugLogger.Log($"Playing one shot attached to {emitter.name}");
        }

        public void PlayOneShot(string path) {
            if (CheckIfStringIsEmpty(path)) return;

            RuntimeManager.PlayOneShot(path);

            debugLogger.Log("Playing one shot global");
        }

        public void PlaySfx(string sfx) {
            PlayOneShot(sfxPath + sfx);
        }

        public void PlaySfx(string sfx, GameObject emitter) {
            PlayOneShotAttached(sfxPath + sfx, emitter);
        }

        public void PlayMusic(string path) {
            if (CheckIfStringIsEmpty(path)) return;

            _currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _currentMusic = RuntimeManager.CreateInstance(path);
            _currentMusic.start();

            debugLogger.Log("Playing new music");
        }

        public void SetParameterValue(string parameter, float value, [Optional] bool ignoreSeekSpeed) {
            if (CheckIfStringIsEmpty(parameter)) return;

            _currentMusic.setParameterByName(parameter, value, ignoreSeekSpeed);

            debugLogger.Log($"Parameter {parameter} set to {value}");
        }

        public void StopMusic() {
            _currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            debugLogger.Log("Stopped music");
        }

        public void StopAllSound() {
            _masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            debugLogger.Log("Stopped all sound");
        }

        public bool IsMusicPlaying() {
            return _currentMusic.GetPlaybackState() == PLAYBACK_STATE.PLAYING;
        }
        #endregion

        private bool CheckIfStringIsEmpty(string s) {
            if (!string.IsNullOrWhiteSpace(s)) return false;
            debugLogger.Log("String received is null or empty");
            return true;
        }
    }
}