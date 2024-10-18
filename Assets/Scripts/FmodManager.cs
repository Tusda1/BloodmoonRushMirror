using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class FmodManager : PersistentSingletonLazyLoad<FmodManager> {
    private EventInstance _currentMusic;
    private Bus _musicBus;
    private Bus _sfxBus;
    private Bus _masterBus;

    public float masterVolume = 1;
    public float musicVolume = 1;
    public float sfxVolume = 1;

    #region ---UnityEventFunctions---

    private IEnumerator Start() {
        yield return new WaitUntil(() => RuntimeManager.IsInitialized);
        _masterBus = RuntimeManager.GetBus("bus:/");
        _musicBus = RuntimeManager.GetBus("bus:/Music Bus");
        _sfxBus = RuntimeManager.GetBus("bus:/Sfx Bus");
    }

    private void Update() {
        _masterBus.setVolume(masterVolume);
        _sfxBus.setVolume(sfxVolume);
        _musicBus.setVolume(musicVolume);
    }

    #endregion

    #region ---PublicFunctions---

    public void PlayOneShotAttached(string path, GameObject emitter) {
        RuntimeManager.PlayOneShotAttached(path, emitter);
    }

    public void PlayOneShot(string path) {
        if (!string.IsNullOrWhiteSpace(path)) {
            RuntimeManager.PlayOneShot(path);
        }
    }

    public void PlayMusic(string path) {
        _currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _currentMusic = RuntimeManager.CreateInstance(path);
        _currentMusic.start();
    }

    public void SetParameterValue(string parameter, float value, [Optional] bool ignoreSeekSpeed) {
        _currentMusic.setParameterByName(parameter, value, ignoreSeekSpeed);
    }

    public void StopMusic() {
        _currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void StopAllSound() {
        _masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public bool IsMusicPlaying() {
        return _currentMusic.GetPlaybackState() == PLAYBACK_STATE.PLAYING;
    }

    #endregion
}