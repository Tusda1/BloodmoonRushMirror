using Animancer;
using FMODScripts;
using Player;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Triggers;
using UnityEngine;
using Utilities.DataBinding;
using Zenject;
using Action = System.Action;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelCheckpoint : TriggerBox, IBind<CheckpointData> {
    #region ---Fields---
    
    [SerializeField] private string raiseSoundEventPath = "event:/Sfx/Misc/Checkpoint_reached";
    [SerializeField] private AnimationClip inactiveIdle;
    [SerializeField] private AnimationClip activeIdle;
    [SerializeField] private AnimationClip activeStart;

    public event Action CheckpointReached = delegate { };
    private AnimancerComponent _animancerComponent;

    #region ***---GameManagers---***
    private SaveLoadSystemManager SaveLoadSystemManager {
        get {
            if (_saveLoadSystemManager != null) {
                return _saveLoadSystemManager;
            }
                
            if (SaveLoadSystemManager.HasInstance) {
                _saveLoadSystemManager = SaveLoadSystemManager.Instance;
                return _saveLoadSystemManager;
            }
                
            throw new NullReferenceException("SaveLoadSystemManager is null");
        }
    }
    [Inject] private SaveLoadSystemManager _saveLoadSystemManager;
    private static LevelDataManager LevelDataManager => LevelDataManager.LevelDataManagerCache;
    #endregion

    #region ***---DataBindingVariables---***
    [field: Header("Data Binding")]
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public CheckpointData SaveSystemData { get; set; }

    public CheckpointData DataInGameData => LevelDataManager.DataInGameData.checkpointDatas.GetOrAddByIdInList(ID,
        new CheckpointData {
            name = name,
            ID = ID,
            HasBeenTriggered = hasBeenTriggered,
            Position = transform.position
        });
    #endregion
    #endregion

    private void Start() {
        Bind();
        
        _animancerComponent = GetComponent<AnimancerComponent>();
        _animancerComponent.Play(hasBeenTriggered ? activeIdle : inactiveIdle);
    }

    protected override void TriggeredAction(Collider2D other) {
        if (raiseSoundEventPath != string.Empty) {
            FMODManager.RaiseEvent(new PlaySoundEvent(raiseSoundEventPath));
        }

        CheckpointReached.Invoke();

        var state = _animancerComponent.Play(activeStart);
        state.Events(this).OnEnd = () => _animancerComponent.Play(activeIdle);

        other.GetComponent<PlayerScript>().CheckpointHealing();
        
        LevelDataManager.CurrentCheckpointData.ID = ID;
        LevelDataManager.CurrentCheckpointData.Position = transform.position;
        SaveLoadSystemManager?.SaveBindings();
    }

    #region ---DataBindingMethods---
    public void Bind() {
        if (SaveLoadSystemManager != null || !Application.isPlaying) {
            SaveLoadSystemManager.Bind(this, DataInGameData);
        }
        else {
            Debug.LogWarning("No SaveLoadSystemManager found", this);
        }
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (gameObject.activeInHierarchy && string.IsNullOrEmpty(ID)) {
            ID = Guid.NewGuid().ToString();
            Debug.Log($"Setting new guid for {nameof(LevelDataManager)}");
        }
    }
#endif

    [ContextMenu(nameof(LoadBindData))]
    public void LoadBindData() {
        name = DataInGameData.name;
        hasBeenTriggered = DataInGameData.HasBeenTriggered;
        transform.position = DataInGameData.Position;
#if UNITY_EDITOR
        if (!Application.isPlaying) {
            EditorUtility.SetDirty(this);
        }
#endif
    }

    [ContextMenu(nameof(SaveBindData))]
    public void SaveBindData() {
        DataInGameData.name = name;
        DataInGameData.HasBeenTriggered = hasBeenTriggered;
        DataInGameData.Position = transform.position;
    }
    #endregion
}