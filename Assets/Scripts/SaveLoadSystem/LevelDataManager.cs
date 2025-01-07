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
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public SceneCollectionSO SceneCollection { get; set; }
        
        [field: SerializeField] public CheckpointData CurrentCheckpointData { get; set; }
        [SerializeReference] public List<CheckpointData> checkpointDatas = new();
        
        [field: SerializeField] public List<EnemyData> enemyDatas = new();

        [field: SerializeField] public LevelData SaveSystemData { get; set; }

        public LevelData DataInGameData => SaveLoadSystemManager.gameData.levelDatas.GetOrAddByIdInList(ID,
            new LevelData {
                ID = ID,
                SceneCollection = SceneCollection,
                enemyDatas = enemyDatas,
                checkpointDatas = checkpointDatas,
                CurrentCheckpointData = CurrentCheckpointData
            });
        #endregion

        private PlayerScript PlayerScriptCache {
            get {
                if (_playerScript != null) {
                    Debug.Log("PlayerScript found in cache");
                    return _playerScript;
                }
                
                var playerScriptLocator = ServiceLocator.Get<PlayerScript>();
                if (playerScriptLocator != null) {
                    Debug.Log("PlayerScript found with ServiceLocator");
                    _playerScript = playerScriptLocator; 
                    return _playerScript;
                }
                
                var playerScriptTag = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
                if (playerScriptTag != null) {
                    Debug.Log("PlayerScript found by tag");
                    _playerScript = playerScriptTag;
                    return _playerScript;
                }
                
                throw new NullReferenceException();
            }
        }
        private PlayerScript _playerScript;
        
        public static LevelDataManager LevelDataManagerCache {
            get {
                if (_levelDataManager != null) {
                    Debug.Log("LevelDataManager found from cache");
                    return _levelDataManager;
                }

                if (HasInstance) {
                    Debug.Log("LevelDataManager found from singleton instance");
                    _levelDataManager = Instance;
                    if (!Application.isPlaying) {
                        _levelDataManager.Bind();
                    }
                    return _levelDataManager;
                }

                var tag = GameObject.FindWithTag("LevelDataManager").GetComponent<LevelDataManager>();
                if (tag != null) {
                    Debug.Log("LevelDataManager found by tag");
                    _levelDataManager = tag;
                    if (!Application.isPlaying) {
                        _levelDataManager.Bind();
                    }
                    return _levelDataManager;
                }

                throw new NullReferenceException();
            }
        }
        private static LevelDataManager _levelDataManager;
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
            SaveSystemData.LevelStartPoint = PlayerScriptCache.transform.position;
        }
#endif

        [ContextMenu(nameof(LoadBindData))]
        public void LoadBindData() {
            if (SaveSystemData.CurrentCheckpointData.ID == string.Empty) {
                PlayerScriptCache.transform.position = SaveSystemData.LevelStartPoint;
            }
            else {
                PlayerScriptCache.transform.position = SaveSystemData.CurrentCheckpointData.Position;
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