using System;
using System.Collections.Generic;
using ExtensionMethods;
using SaveLoadSystem.Data;
using SceneCollections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveLoadSystem {
    public partial class LevelDataManager {
        #region ---Fields---
        #region ***---DataBindingVariables---***
        [field: Header("Data Binding")]
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public SceneCollectionSO SceneCollection { get; set; }
        [field: SerializeField] public Vector3 PlayerPosition { get; set; }
        [field: SerializeField] public string CurrentCheckpointID { get; set; }
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
            levelTimerData = new LevelTimerData()
        };
        #endregion
        
        public SaveGameData currentGameData;
        #endregion

        private void UnbindAll(Scene arg0) {
            if (arg0.name is not ("EntranceHall" or "TheDungeons" or "TheBoiler" or "CastleMain" or "ToweringHeights" or "DraculasOffice")) return;
            
            foreach (var dataBinding in _saveLoadSystemManager.DataBinder.GameDataBinds.ToArray()) {
                dataBinding.Unbind();
            }
        }

        #region ---DataBindingMethods---
        public void Bind() {
            currentGameData = _saveLoadSystemManager.CurrentGameData;

            SaveBindData();
            
            _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
        }

        public void LoadBindData() {
            _playerScript.transform.position = DataInGameData.PlayerPosition;
        }

        public void Unbind() {
            _saveLoadSystemManager.DataBinder.UnBind(this, DataInGameData);
        }

        public void LoadCheckpointBindData() => LoadBindData();

        public void SaveBindData() {
            _saveLoadSystemManager.CurrentGameData.currentSceneCollectionName = SceneCollection.name;
        }
        #endregion
    }
}