using System;
using System.Collections.Generic;
using FMODScripts;
using Triggers;
using UnityEngine;
using Animancer;
using Player;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Utilities.DataBinding;

public class LevelCheckpoint : TriggerBox, IBind<CheckpointData> {
    #region ---Fields---
    [SerializeField] private string raiseSoundEventPath = "event:/Sfx/Misc/Checkpoint_reached";
    [SerializeField] private AnimationClip inactiveIdle;
    [SerializeField] private AnimationClip activeIdle;
    [SerializeField] private AnimationClip activeStart;

    public event Action CheckpointReached = delegate { };
    private AnimancerComponent _animancerComponent;

    #region ***---GameManagers---***
    private static SaveLoadSystemManager SaveLoadSystemManager => SaveLoadSystemManager.Instance;
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
            HasBeenTriggered = HasBeenTriggered,
            Position = transform.position
        });
    #endregion
    #endregion

    private void Start() {
        _animancerComponent = GetComponent<AnimancerComponent>();
        _animancerComponent.Play(inactiveIdle);
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
        SaveLoadSystemManager.SaveBindings();
    }

    #region ---DataBindingMethods---
    public void Bind() {
        if (SaveLoadSystemManager != null) {
            SaveLoadSystemManager.Bind(this, DataInGameData);
        }
        else {
            Debug.LogWarning("No SaveLoadSystemManager found");
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
        HasBeenTriggered = DataInGameData.HasBeenTriggered;
        transform.position = DataInGameData.Position;
    }

    [ContextMenu(nameof(SaveBindData))]
    public void SaveBindData() {
        DataInGameData.name = name;
        DataInGameData.HasBeenTriggered = HasBeenTriggered;
        DataInGameData.Position = transform.position;
    }
    #endregion
}