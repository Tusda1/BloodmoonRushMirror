using System;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;
using VContainer;

public class LevelMusicManager : MonoBehaviour, IBind<MusicData> {
    #region ---Fields---
    [Header(nameof(LevelMusicManager))]
    [SerializeField] private string musicID;
    [SerializeField] private float calmnessParameter;

    public IAudioManager AudioManager;
    private LevelDataManager _levelDataManager;
    private SaveLoadSystemManager _saveLoadSystemManager;

    #region ***---DataBinding---***
    [field: Header("Data Binding")]
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public MusicData SaveSystemData { get; set; }
    public MusicData DataInGameData => _levelDataManager.SaveSystemData.MusicData;
    #endregion

    public const string CalmnessParameter = "is_chill";
    #endregion

    #region ---Initialization---
    [Inject]
    public void Construct(IObjectResolver container) {
        AudioManager = container.Resolve<IAudioManager>();
        _levelDataManager = container.Resolve<LevelDataManager>();
        _saveLoadSystemManager = container.Resolve<SaveLoadSystemManager>();
    }

    private void Start() {
        Bind();
        
        if (SaveSystemData.ID == "") SaveBindData();
        LoadBindData();
    }
    #endregion

    public void SetCalmnessParameter(float value) {
        calmnessParameter = value;
        AudioManager.SetParameterValue(CalmnessParameter, calmnessParameter);
    }

    public void PlayMusic(string soundID) {
        musicID = soundID;
        AudioManager.PlayMusic(soundID);
    }

    public void Stop() {
        AudioManager.StopAllMusic();
    }
    
    public void Play() {
        if (string.IsNullOrWhiteSpace(musicID)) throw new NullReferenceException("MusicID cannot be null!");
        
        PlayMusic(musicID);
    }

    #region ---DataBindingMethods---
    public void Bind() {
        _saveLoadSystemManager.Bind(this, DataInGameData);
    }

    [ContextMenu(nameof(LoadBindData))]
    public void LoadBindData() {
        SetCalmnessParameter(SaveSystemData.CalmnessParameter);
        PlayMusic(SaveSystemData.MusicID);
    }

    [ContextMenu(nameof(SaveBindData))]
    public void SaveBindData() {
        SaveSystemData.ID = ID;
        SaveSystemData.MusicID = musicID;
        SaveSystemData.CalmnessParameter = calmnessParameter;
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (!gameObject.activeInHierarchy || !string.IsNullOrEmpty(ID)) return;

        ID = Guid.NewGuid().ToString();
        Debug.Log($"Setting new guid for {name}");
    }
#endif
    #endregion
}