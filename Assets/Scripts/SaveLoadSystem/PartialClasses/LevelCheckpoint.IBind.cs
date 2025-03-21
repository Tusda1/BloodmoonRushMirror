using System;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using UnityEngine;

public partial class LevelCheckpoint {
    #region ---Fields---
    [field: Header("Data Binding")]
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public CheckpointData SaveSystemData { get; set; }

    public CheckpointData DataInGameData => _levelDataManager.DataInGameData.checkpointDatas
        .GetOrAddByIDInList(ID, CheckpointDataGenerator);
    
    private Func<CheckpointData> CheckpointDataGenerator => () => new CheckpointData {
        name = name,
        ID = ID,
        HasBeenTriggered = hasBeenTriggered,
        Position = transform.position
    };
    #endregion
    
    private void OnDestroy() {
        Unbind();
    }

    #region ---DataBindingMethods---
    public void Bind() {
        _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
    }

    public void Unbind() {
        _saveLoadSystemManager.DataBinder.UnBind(this, DataInGameData);
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

        if (hasBeenTriggered) {
            animancerComponent.Play(activeIdle);
            _lightComponent.pointLightOuterRadius = _lightRange;
        }
    }

    [ContextMenu(nameof(SaveBindData))]
    public void SaveBindData() {
        DataInGameData.name = name;
        DataInGameData.HasBeenTriggered = hasBeenTriggered;
        DataInGameData.Position = transform.position;
    }
    #endregion
}