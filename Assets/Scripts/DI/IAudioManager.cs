using UnityEngine;

public interface IAudioManager {
    #region ---Play---
    void PlaySFX(string name);
    void PlaySFXAttached(string name, GameObject gameObject);
    
    void PlayMusic(string name);
    void PlayMusicAttached(string name, GameObject gameObject);
    #endregion
    
    #region ---Stop---
    void StopSFX(string name);
    void StopAllSFX();
    
    void StopMusic(string name);
    void StopAllMusic();
    
    void StopAllSound();
    #endregion
    
    #region ---Volume---
    float SFXVolume { get; set; }
    float MusicVolume { get; set; }
    float MasterVolume { get; set; }
    
    void SetSFXVolume(float volume);
    void SetMusicVolume(float volume);
    void SetMasterVolume(float volume);
    
    void SetVolume(string id, float volume);
    #endregion
    
    #region ---Query---
    bool IsSFXPlaying(string name);
    
    bool IsMusicPlaying();
    bool IsMusicPlaying(string name);
    #endregion
}