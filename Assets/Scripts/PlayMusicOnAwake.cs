using UnityEngine;
using VContainer;

public class PlayMusicOnAwake : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private string musicID;
    [SerializeField] private int musicStage;
    
    [Inject] private IAudioManager _audioManager;
    #endregion
    
    private void Start() {
        _audioManager.PlayMusic(musicID);
        _audioManager.SetParameterValue(IAudioManager.CalmnessParameter, musicStage);
    }
}
