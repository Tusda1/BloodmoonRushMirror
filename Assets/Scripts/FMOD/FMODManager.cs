using System.Collections.Generic;
using System.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using VContainer;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace FMODScripts {
    public class FMODManager : IAudioManager {
        #region ---Fields---
        private float _sfxVolume;
        private float _musicVolume;
        private float _masterVolume;

        private EventInstance _currentMusic;
        private Bus _musicBus;
        private Bus _sfxBus;
        private Bus _masterBus;

        private readonly DebugLoggerSO _debugLogger;
        private readonly AudioDictionarySO _audioDictionary;

        public string CurrentMusicID { get; private set; }

        #region ***---Consts---***
        private const string BUS_PATH = "bus:/";
        private const string SFX_BUS_PATH = BUS_PATH + "Sfx Bus";
        private const string MUSIC_BUS_PATH = BUS_PATH + "Music Bus";

        private const string MASTER_PLAYER_PREFS_KEY = "MasterVolume";
        private const string SFX_PLAYER_PREFS_KEY = "SFXVolume";
        private const string MUSIC_PLAYER_PREFS_KEY = "MusicVolume";

        private const float DEFAULT_VOLUME = 1.0f;
        #endregion
        #endregion

        #region ---Initialization---
        [Inject]
        public FMODManager(IObjectResolver container) {
            _audioDictionary = container.Resolve<AudioDictionarySO>();
            _debugLogger = container.ResolveProviderID<DebugLoggerSO>("FMODManager");
            
            _ = Initialize();
        }

        private async Task Initialize() {
            _debugLogger.Log("Initializing");
            
            RuntimeManager.MuteAllEvents(true);
            while (!RuntimeManager.IsInitialized) {
                await Task.Delay(1);
            }
            RuntimeManager.MuteAllEvents(false);

            _debugLogger.Log("RuntimeManager initialized");

            _masterBus = RuntimeManager.GetBus(BUS_PATH);
            _sfxBus = RuntimeManager.GetBus(SFX_BUS_PATH);
            _musicBus = RuntimeManager.GetBus(MUSIC_BUS_PATH);

            if (_masterBus.Equals(default) || _sfxBus.Equals(default) || _musicBus.Equals(default)) {
                _debugLogger.Log("Busses are not valid");
            }

            MasterVolume = GetVolumeFromPlayerPrefs(MASTER_PLAYER_PREFS_KEY);
            SFXVolume = GetVolumeFromPlayerPrefs(SFX_PLAYER_PREFS_KEY);
            MusicVolume = GetVolumeFromPlayerPrefs(MUSIC_PLAYER_PREFS_KEY, 0.5f);
        }

        private static float GetVolumeFromPlayerPrefs(string name, float defaultValue = 1) {
            if (PlayerPrefs.HasKey(name)) {
                return PlayerPrefs.GetFloat(name);
            }

            PlayerPrefs.SetFloat(name, DEFAULT_VOLUME);
            return DEFAULT_VOLUME;
        }
        #endregion

        #region ---Play---
        public void PlayOneShot(string id) {
            if (CheckIfStringIsEmpty(id)) return;

            var path = IDToPath(id);

            if (path.Equals(string.Empty)) {
                _debugLogger.ForceLogWarning("NO SFX BY GIVEN ID: " + id);
            }
            else {
                RuntimeManager.PlayOneShot(path);

                _debugLogger.Log($"Playing SFX: {id} global");
            }
        }

        public void PlayOneShotAttached(string id, GameObject gameObject) {
            if (CheckIfStringIsEmpty(id)) return;

            var path = IDToPath(id);

            if (path.Equals(string.Empty)) {
                _debugLogger.ForceLogWarning("NO SFX BY GIVEN ID: " + id);
            }
            else {
                RuntimeManager.PlayOneShotAttached(path, gameObject);

                _debugLogger.Log($"Playing SFX: {id} attached to {gameObject.name}");
            }
        }
        
        public EventInstance CreateEventInstance(string id) {
            var path = IDToPath(id);

            return RuntimeManager.CreateInstance(path);
        }

        public void PlayMusic(string id) {
            if (CheckIfStringIsEmpty(id)) return;
            
            var path = IDToPath(id);

            _currentMusic.stop(STOP_MODE.ALLOWFADEOUT);

            if (path.Equals(string.Empty)) {
                _debugLogger.ForceLogWarning("NO MUSIC BY GIVEN ID! | " + id);
            }
            else {
                _currentMusic = RuntimeManager.CreateInstance(path);
                RuntimeManager.DetachInstanceFromGameObject(_currentMusic);
                _currentMusic.start();
                CurrentMusicID = id;
                
                _debugLogger.Log("Playing music: " + id);
            }
        }

        public void PlayMusicAttached(string id, GameObject gameObject) {
            if (CheckIfStringIsEmpty(id)) return;
            
            var path = IDToPath(id);

            _currentMusic.stop(STOP_MODE.ALLOWFADEOUT);
            
            if (path.Equals(string.Empty)) {
                _debugLogger.ForceLogWarning("NO MUSIC BY GIVEN ID! | " + id);
            }
            else {
                _currentMusic = RuntimeManager.CreateInstance(id);
                RuntimeManager.AttachInstanceToGameObject(_currentMusic, gameObject);
                _currentMusic.start();
                CurrentMusicID = id;

                _debugLogger.Log("Playing music: " + id);
            }
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

        public void StopMusic(string name) {
            if (CurrentMusicID.Equals(name)) {
                StopAllMusic();
            }
        }

        public void StopAllMusic() {
            _currentMusic.stop(STOP_MODE.ALLOWFADEOUT);
            CurrentMusicID = "";

            _debugLogger.Log("Stopped all music");
        }

        public void StopAllSound() {
            _masterBus.stopAllEvents(STOP_MODE.ALLOWFADEOUT);

            _debugLogger.Log("Stopped all sound");
        }
        #endregion

        #region ---SetVolume---
        public float MasterVolume {
            get => _masterVolume;
            set {
                _masterVolume = Mathf.Clamp01(value);
                
                _masterBus.setVolume(MasterVolume);
                PlayerPrefs.SetFloat(MASTER_PLAYER_PREFS_KEY, MasterVolume);
            }
        }
        
        public float SFXVolume {
            get => _sfxVolume;
            set {
                _sfxVolume = Mathf.Clamp01(value);

                _sfxBus.setVolume(SFXVolume);
                PlayerPrefs.SetFloat(SFX_PLAYER_PREFS_KEY, SFXVolume);
            }
        }

        public float MusicVolume {
            get => _musicVolume;
            set {
                _musicVolume = Mathf.Clamp01(value);

                _musicBus.setVolume(MusicVolume);
                PlayerPrefs.SetFloat(MUSIC_PLAYER_PREFS_KEY, MusicVolume);
            }
        }

        public void SetSFXVolume(float volume) => SFXVolume = volume;

        public void SetMusicVolume(float volume) => MusicVolume = volume;

        public void SetMasterVolume(float volume) => MasterVolume = volume;

        public void SetVolume(string id, float volume) {
            throw new System.NotImplementedException();
        }
        #endregion

        #region ---Query---
        /// <summary>
        /// NB! NotImplemented
        /// </summary>
        public bool IsSFXPlaying(string id) {
            throw new System.NotImplementedException();
        }

        public bool IsMusicPlaying() {
            return _currentMusic.GetPlaybackState() == PLAYBACK_STATE.PLAYING;
        }

        public bool IsMusicPlaying(string id) {
            return CurrentMusicID.Equals(id);
        }
        #endregion

        public void SetParameterValue(string parameter, float value, bool ignoreSeekSpeed = false) {
            if (CheckIfStringIsEmpty(parameter)) return;

            RuntimeManager.StudioSystem.setParameterByName(parameter, value, ignoreSeekSpeed);

            _debugLogger.Log($"Parameter {parameter} set to {value}");
        }

        private bool CheckIfStringIsEmpty(string s) {
            if (!string.IsNullOrWhiteSpace(s)) return false;
            _debugLogger.ForceLogWarning("String received was null or empty");
            return true;
        }

        private string IDToPath(string id) {
#if UNITY_EDITOR
            try {
#endif
                return _audioDictionary.dictionary[id];
#if UNITY_EDITOR
            }
            catch (KeyNotFoundException) {
                throw new KeyNotFoundException($"ID: '{id}' not found in AudioDictionary!");
            }
#endif
        }
    }
}