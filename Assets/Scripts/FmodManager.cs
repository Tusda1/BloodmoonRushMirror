using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using Unity.VisualScripting;

public class FmodManager : PersistentSingletonLazyLoad<FmodManager>
{
    private EventInstance _currentMusic;
    private Bus _musicBus;
    private Bus _sfxBus;
    private Bus _masterBus;
    
    public float masterVolume = 1 ;
    public float musicVolume = 1;
    public float sfxVolume = 1;
    
    public void PlayOneShotAttached(string path, GameObject emitter) //plays a sfx attached to an object
    {
        RuntimeManager.PlayOneShotAttached(path, emitter);
    }

    public void PlayOneShot(string path) //globally plays a sfx
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            RuntimeManager.PlayOneShot(path);
        }
    }

    public void PlayMusic(string path) //plays music
    {
        _currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _currentMusic = RuntimeManager.CreateInstance(path);
        _currentMusic.start();
    }

    public bool IsMusic()
    {
        if (PlaybackState(_currentMusic) == FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    FMOD.Studio.PLAYBACK_STATE PlaybackState(FMOD.Studio.EventInstance instance)
    {
        FMOD.Studio.PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        return state;
    }

    public void SetParameterValue(string parameter, float value, [Optional] bool ignoreSeekSpeed)
    {
        _currentMusic.setParameterByName(parameter, value, ignoreSeekSpeed);
    }

    public void StopMusic() //stops music
    {
        _currentMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    
    public void StopSound() //stops sound
    {
        _masterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
 
    private void Start()
    {
        StartCoroutine(InitialiseMusicBusses());
    }

    private void Update()
    {
        _masterBus.setVolume(masterVolume);
        _sfxBus.setVolume(sfxVolume);
        _musicBus.setVolume(musicVolume);
    }

    private IEnumerator InitialiseMusicBusses()
    {
        yield return new WaitUntil(() => RuntimeManager.IsInitialized);
        _masterBus = RuntimeManager.GetBus("bus:/");
        _musicBus = RuntimeManager.GetBus("bus:/Music Bus");
        _sfxBus = RuntimeManager.GetBus("bus:/Sfx Bus");
    }
}
