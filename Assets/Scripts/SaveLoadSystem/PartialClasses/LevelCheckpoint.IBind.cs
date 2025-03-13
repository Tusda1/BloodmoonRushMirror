using System;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    #region ---DataBindingMethods---
    public void Bind() {
#if SAVESYSTEM_ACTIVE
        _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
#endif
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
    }

    [ContextMenu(nameof(SaveBindData))]
    public void SaveBindData() {
        DataInGameData.name = name;
        DataInGameData.HasBeenTriggered = hasBeenTriggered;
        DataInGameData.Position = transform.position;
    }
    #endregion
}