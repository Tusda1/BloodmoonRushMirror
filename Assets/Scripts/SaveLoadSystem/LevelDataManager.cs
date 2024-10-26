using System;
using System.Linq;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;

public class LevelDataManager : Singleton<LevelDataManager>, IBind<LevelData> {
    #region ---Fields---
    [Header("Debug Logger"), SerializeField, Attributes.Required]
    private DebugLoggerSO debugLogger;

    #region ***---GameManagers---***
    private static SaveLoadSystemManager saveLoadSystemManager => SaveLoadSystemManager.Instance;
    #endregion

    #region ***---DataBindingVariables---***
    [field: Header("Data Binding")]
    [field: SerializeField] public string id { get; set; } = Guid.NewGuid().ToString();
    [field: SerializeField] public LevelData saveSystemData { get; set; }
    public LevelData dataInGameData {
        get {
            var levelData = saveLoadSystemManager.gameData.levelData.FirstOrDefault(x => x.id == id);

            if (levelData != null) return levelData;
            
            levelData = new LevelData { id = id };
            saveLoadSystemManager.gameData.levelData.Add(levelData);

            return levelData;
        }
    }
    #endregion
    #endregion

    protected override void Awake() {
        base.Awake();

        debugLogger.messageOrigin = this;
    }

    private void Start() {
        Bind();
    }

    #region ---DataBindingMethods---
    
    public void Bind() {
        saveLoadSystemManager.Bind(this, dataInGameData);
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (gameObject.activeInHierarchy && string.IsNullOrEmpty(id)) {
            id = Guid.NewGuid().ToString();
            Debug.Log($"Setting new guid for {nameof(TestEnemySaveable)}");
        }
    }
#endif

    [ContextMenu(nameof(LoadBindData))]
    public void LoadBindData() {
    }
    
    [ContextMenu(nameof(SaveBindData))]
    public void SaveBindData() {
    }
    #endregion
}