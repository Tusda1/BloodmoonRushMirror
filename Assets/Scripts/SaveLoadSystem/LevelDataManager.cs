using System;
using System.Collections.Generic;
using Player;
using SaveLoadSystem.Data;
using SceneCollections;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem {
    public class LevelDataManager : Singleton<LevelDataManager>, IBind<LevelData> {
        #region ---Fields---
        #region ***---GameManagers---***
        private static SaveLoadSystemManager SaveLoadSystemManager => SaveLoadSystemManager.Instance;
        #endregion

        #region ***---DataBindingVariables---***
        [field: Header("Data Binding")]
        [field: SerializeField] public string ID { get; set; } = Guid.NewGuid().ToString();
        [field: SerializeField] public SceneCollectionSO SceneCollection { get; set; }
        [field: SerializeField] public CurrentCheckpointData Checkpoint { get; set; }
        
        [field: SerializeField] public List<EnemyData> enemyDatas = new();

        [field: SerializeField] public LevelData SaveSystemData { get; set; }

        public LevelData DataInGameData => SaveLoadSystemManager.gameData.levelDatas.GetOrAddByIdInList(ID,
            new LevelData {
                ID = ID,
                SceneCollection = SceneCollection,
                enemyDatas = enemyDatas
            });
        #endregion

        private PlayerScript PlayerScript {
            get {
                if (_playerScript != null) {
                    Debug.Log("PlayerScript found in cache");
                    return _playerScript;
                }
                
                var locator = ServiceLocator.Get<PlayerScript>();
                if (locator != null) {
                    Debug.Log("PlayerScript found with ServiceLocator");
                    _playerScript = locator; 
                    return _playerScript;
                }
                
                var tag = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
                if (tag != null) {
                    Debug.Log("PlayerScript found by tag");
                    _playerScript = tag;
                    return _playerScript;
                }
                
                throw new NullReferenceException();
            }
        }
        private PlayerScript _playerScript;
        #endregion

        private void Start() {
            Bind();
        }

        #region ---DataBindingMethods---

        public void Bind() {
            SaveLoadSystemManager.Bind(this, DataInGameData);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (gameObject.activeInHierarchy && string.IsNullOrEmpty(ID)) {
                ID = Guid.NewGuid().ToString();
                Debug.Log($"Setting new guid for {nameof(LevelDataManager)}");
            }
        }
        
        [ContextMenu(nameof(SavePlayerLevelStartPoint))]
        public void SavePlayerLevelStartPoint() {
            SaveSystemData.LevelStartPoint = PlayerScript.transform.position;
        }
#endif

        [ContextMenu(nameof(LoadBindData))]
        public void LoadBindData() {
            if (SaveSystemData.Checkpoint.ID == string.Empty) {
                PlayerScript.transform.position = SaveSystemData.LevelStartPoint;
            }
            else {
                PlayerScript.transform.position = SaveSystemData.Checkpoint.Position;
            }
        }

        [ContextMenu(nameof(SaveBindData))]
        public void SaveBindData() {
            SaveSystemData.enemyDatas = enemyDatas;
            SaveLoadSystemManager.gameData.CurrentLevel = SaveLoadSystemManager.gameData.levelDatas.FindIndex(x => x.ID == ID);
        }
        #endregion
    }
}