using System.Collections.Generic;
using Player;
using SaveLoadSystem.Data;
using SceneCollections;
using UnityEngine;
using Utilities.DataBinding;
using VContainer;

namespace SaveLoadSystem {
    public class LevelDataManager : Singleton<LevelDataManager>, IBind<LevelData> {
        #region ---Fields---
        #region ***---DataBindingVariables---***
        [field: Header("Data Binding")]
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public SceneCollectionSO SceneCollection { get; set; }
        [field: SerializeField] public Vector3 LevelStartPoint { get; set; }
        [field: SerializeField] public CheckpointData CurrentCheckpointData { get; set; }
        [SerializeReference] public List<CheckpointData> checkpointDatas = new();
        
        [field: SerializeField] public List<EnemyData> enemyDatas = new();

        [field: SerializeField] public LevelData SaveSystemData { get; set; }

        public LevelData DataInGameData => _saveLoadSystemManager.CurrentGameData.levelDatas.GetOrAddByIdInList(ID,
            new LevelData {
                ID = ID,
                SceneCollection = SceneCollection,
                LevelStartPoint = LevelStartPoint,
                enemyDatas = enemyDatas,
                checkpointDatas = checkpointDatas,
                CurrentCheckpointData = CurrentCheckpointData,

            });
        #endregion
        
        private ISaveLoadSystemManager _saveLoadSystemManager;
        private PlayerScript _playerScript;
        #endregion
        
        #region ---Initialization---
        [Inject]
        public void Construct(IObjectResolver container) {
#if SAVESYSTEM_ACTIVE
        _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
#endif
            _playerScript = container.Resolve<PlayerScript>();
        }

        private void Start() {
            Bind();
            
#if SAVESYSTEM_ACTIVE
            _playerScript.LoadBindData();
#else
#if UNITY_EDITOR
        if (SceneCollectionManager.CurrentSceneCollection == null) {
#endif
        _playerScript.transform.position = LevelStartPoint;
        _playerScript.healthComponent.Health = _playerScript.healthComponent.maxHealth;
        _playerScript.gems = 0;
        _playerScript.playerLives = _playerScript.playerLivesStartAmount;
#if UNITY_EDITOR
        }
#endif
#endif
        }
        #endregion

        #region ---DataBindingMethods---
        public void Bind() {
#if SAVESYSTEM_ACTIVE
            _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
#endif
        }

        [ContextMenu(nameof(LoadBindData))]
        public void LoadBindData() {
        }

        [ContextMenu(nameof(SaveBindData))]
        public void SaveBindData() {
        }
        #endregion
    }
}