using UnityEngine;

public interface IAudioManager {
    string CurrentMusicID { get; }

    #region ---Play---
    void PlayOneShot(string id);
    void PlayOneShotAttached(string id, GameObject gameObject);

    void PlayMusic(string id);
    void PlayMusicAttached(string id, GameObject gameObject);
    #endregion

    #region ---Stop---
    void StopSFX(string name);
    void StopAllSFX();

    void StopMusic(string name);
    void StopAllMusic();

    void StopAllSound();
    #endregion

    #region ---Volume---
    float MasterVolume { get; set; }
    float SFXVolume { get; set; }
    float MusicVolume { get; set; }

    void SetSFXVolume(float volume);
    void SetMusicVolume(float volume);
    void SetMasterVolume(float volume);

    void SetVolume(string id, float volume);
    #endregion

    #region ---Query---
    bool IsSFXPlaying(string id);

    bool IsMusicPlaying();
    bool IsMusicPlaying(string id);
    #endregion

    void SetParameterValue(string parameter, float value, bool ignoreSeekSpeed = false);
}