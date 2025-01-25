using System.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace FMODScripts {
    public class FMODManagerDI : IAudioManager {
        #region ---Fields---
        private float _sfxVolume;
        private float _musicVolume;
        private float _masterVolume;

        private EventInstance _currentMusic;
        private Bus _musicBus;
        private Bus _sfxBus;
        private Bus _masterBus;

        private readonly DebugLoggerSO _debugLogger;

        #region ***---Consts---***
        private const string BusPath = "bus:/";
        private const string SFXBusPath = BusPath + "Sfx Bus";
        private const string MusicBusPath = BusPath + "Music Bus";

        private const string MasterPlayerPrefsKey = "MasterVolume";
        private const string SFXPlayerPrefsKey = "SFXVolume";
        private const string MusicPlayerPrefsKey = "MusicVolume";

        private const float DefaultVolume = 1.0f;
        #endregion
        #endregion

        #region ---Initialization---
        public FMODManagerDI(DebugLoggerSO debugLogger) {
            _debugLogger = debugLogger;
            _ = Initialize();
        }

        private async Task Initialize() {
            _debugLogger.Log("Initializing");

            while (!RuntimeManager.IsInitialized) {
                await Task.Delay(100);
            }

            _debugLogger.Log("RuntimeManager initialized");

            _masterBus = RuntimeManager.GetBus(BusPath);
            _sfxBus = RuntimeManager.GetBus(SFXBusPath);
            _musicBus = RuntimeManager.GetBus(MusicBusPath);

            if (_masterBus.Equals(default) || _sfxBus.Equals(default) || _musicBus.Equals(default)) {
                _debugLogger.Log("Busses are not valid");
            }

            MasterVolume = GetVolumeFromPlayerPrefs(MasterPlayerPrefsKey);
            SFXVolume = GetVolumeFromPlayerPrefs(SFXPlayerPrefsKey);
            MusicVolume = GetVolumeFromPlayerPrefs(MusicPlayerPrefsKey);
        }

        private static float GetVolumeFromPlayerPrefs(string name) {
            if (PlayerPrefs.HasKey(name)) {
                return PlayerPrefs.GetFloat(name);
            }

            PlayerPrefs.SetFloat(name, DefaultVolume);
            return DefaultVolume;
        }
        #endregion

        #region ---Play---
        public void PlaySFX(string name) {
            if (CheckIfStringIsEmpty(name)) return;

            RuntimeManager.PlayOneShot(name);

            _debugLogger.Log($"Playing SFX: {name} global");
        }

        public void PlaySFXAttached(string name, GameObject gameObject) {
            if (CheckIfStringIsEmpty(name)) return;

            RuntimeManager.PlayOneShotAttached(name, gameObject);

            _debugLogger.Log($"Playing SFX: {name} attached to {gameObject.name}");
        }

        public void PlayMusic(string name) {
            if (CheckIfStringIsEmpty(name)) return;

            _currentMusic.stop(STOP_MODE.ALLOWFADEOUT);
            _currentMusic = RuntimeManager.CreateInstance(name);
            RuntimeManager.DetachInstanceFromGameObject(_currentMusic);
            _currentMusic.start();

            _debugLogger.Log("Playing music: " + name);
        }

        public void PlayMusicAttached(string name, GameObject gameObject) {
            if (CheckIfStringIsEmpty(name)) return;

            _currentMusic.stop(STOP_MODE.ALLOWFADEOUT);
            _currentMusic = RuntimeManager.CreateInstance(name);
            RuntimeManager.AttachInstanceToGameObject(_currentMusic, gameObject);
            _currentMusic.start();

            _debugLogger.Log("Playing music: " + name);
        }
        #endregion

        #region ---Stop---
        /// <summary>
        /// NB! NotImplemented
        /// </summary>
        public void StopSFX(string name) {
            throw new System.NotImplementedException();
        }

        public void StopAllSFX() {
            _sfxBus.stopAllEvents(STOP_MODE.ALLOWFADEOUT);
        }

        /// <summary>
        /// NB! NotImplemented
        /// </summary>
        public void StopMusic(string name) {
            throw new System.NotImplementedException();
        }

        public void StopAllMusic() {
            _currentMusic.stop(STOP_MODE.ALLOWFADEOUT);

            _debugLogger.Log("Stopped all music");
        }

        public void StopAllSound() {
            _masterBus.stopAllEvents(STOP_MODE.ALLOWFADEOUT);

            _debugLogger.Log("Stopped all sound");
        }
        #endregion

        #region ---SetVolume---
        public float SFXVolume {
            get => _sfxVolume;
            set {
                _sfxVolume = Mathf.Clamp01(value);

                PlayerPrefs.SetFloat(SFXPlayerPrefsKey, _sfxVolume);
            }
        }

        public float MusicVolume {
            get => _musicVolume;
            set {
                _musicVolume = Mathf.Clamp01(value);

                PlayerPrefs.SetFloat(MusicPlayerPrefsKey, _musicVolume);
            }
        }

        public float MasterVolume {
            get => _masterVolume;
            set {
                _masterVolume = Mathf.Clamp01(value);

                PlayerPrefs.SetFloat(MasterPlayerPrefsKey, _masterVolume);
            }
        }

        public void SetSFXVolume(float volume) {
            SFXVolume = volume;
        }

        public void SetMusicVolume(float volume) {
            MusicVolume = volume;
        }

        public void SetMasterVolume(float volume) {
            MasterVolume = volume;
        }

        public void SetVolume(string id, float volume) {
            throw new System.NotImplementedException();
        }
        #endregion

        #region ---Query---
        /// <summary>
        /// NB! NotImplemented
        /// </summary>
        public bool IsSFXPlaying(string name) {
            throw new System.NotImplementedException();
        }

        public bool IsMusicPlaying() {
            return _currentMusic.GetPlaybackState() == PLAYBACK_STATE.PLAYING;
        }

        /// <summary>
        /// NB! NotImplemented
        /// </summary>
        public bool IsMusicPlaying(string name) {
            throw new System.NotImplementedException();
        }
        #endregion

        private bool CheckIfStringIsEmpty(string s) {
            if (!string.IsNullOrWhiteSpace(s)) return false;
            _debugLogger.ForceLogWarning("String received was null or empty");
            return true;
        }
    }
}