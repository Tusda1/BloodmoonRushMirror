using System;
using System.Collections.Generic;
using SaveLoadSystem.Data;
using SceneCollections;
using UnityEngine;

namespace SaveLoadSystem {
    public partial class LevelDataManager {
        #region ---Fields---
        #region ***---DataBindingVariables---***
        [field: Header("Data Binding")]
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public SceneCollectionSO SceneCollection { get; set; }
        [field: SerializeField] public Vector3 PlayerPosition { get; set; }
        [field: SerializeField] public CheckpointData CurrentCheckpointData { get; set; }
        [SerializeReference] public List<CheckpointData> checkpointDatas = new();

        [field: SerializeField] public List<EnemyData> enemyDatas = new();
        [field: SerializeField] public List<TriggerData> triggerDatas = new();

        [field: SerializeField] public LevelData SaveSystemData { get; set; }

        public LevelData DataInGameData => _saveLoadSystemManager.CurrentGameData.levelDatas
            .GetOrAddByIDInList(ID, LevelDataGenerator);

        private Func<LevelData> LevelDataGenerator => () => new LevelData {
            ID = ID,
            SceneCollection = SceneCollection,
            PlayerPosition = PlayerPosition,
            enemyDatas = enemyDatas,
            checkpointDatas = checkpointDatas,
            CurrentCheckpointData = CurrentCheckpointData,
        };
        #endregion
        
        public SaveGameData currentGameData;
        
        #endregion

        #region ---DataBindingMethods---
        public void Bind() {
#if SAVESYSTEM_ACTIVE
            _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
            currentGameData = _saveLoadSystemManager.CurrentGameData;
#endif
        }

        [ContextMenu(nameof(LoadBindData))]
        public void LoadBindData() {
            _playerScript.transform.position = DataInGameData.CurrentCheckpointData.HasBeenTriggered 
                ? DataInGameData.CurrentCheckpointData.Position 
                : DataInGameData.PlayerPosition;
        }

        public void LoadCheckpointBindData() => LoadBindData();

        [ContextMenu(nameof(SaveBindData))]
        public void SaveBindData() {
            DataInGameData.PlayerPosition = _playerScript.transform.position;
        }
        #endregion
    }
}