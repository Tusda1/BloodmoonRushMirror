using System.Collections.Generic;
using Player;
using SaveLoadSystem.Data;
using SceneCollections;
using UnityEngine;
using Utilities.DataBinding;
using VContainer;

namespace SaveLoadSystem {
    public class LevelDataManager : MonoBehaviour, IBind<LevelData> {
        #region ---Fields---
        #region ***---DataBindingVariables---***
        [field: Header("Data Binding")]
        [field: SerializeField]
        public string ID { get; set; }

        [field: SerializeField] public SceneCollectionSO SceneCollection { get; set; }
        [field: SerializeField] public Vector3 PlayerPosition { get; set; }
        [field: SerializeField] public CheckpointData CurrentCheckpointData { get; set; }
        [SerializeReference] public List<CheckpointData> checkpointDatas = new();

        [field: SerializeField] public List<EnemyData> enemyDatas = new();

        [field: SerializeField] public LevelData SaveSystemData { get; set; }

        public LevelData DataInGameData => _saveLoadSystemManager.CurrentGameData.levelDatas.GetOrAddByIdInList(ID,
            new LevelData {
                ID = ID,
                SceneCollection = SceneCollection,
                PlayerPosition = PlayerPosition,
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
#if UNITY_EDITOR
            if (_saveLoadSystemManager.CurrentGameData == null) {
                _saveLoadSystemManager.SetCurrentGameDataFromTemplate("NOSAVESYSTEM");
            }
#endif
#endif
            _playerScript = container.Resolve<PlayerScript>();
        }

        protected void Start() {
            Bind();
#if SAVESYSTEM_ACTIVE
            if (_saveLoadSystemManager.CurrentGameData == null) {
                _playerScript.LoadBindData();
            }
#else
        if (SceneCollectionManager.CurrentSceneCollection == null) {
            _playerScript.transform.position = LevelStartPoint;
            _playerScript.healthComponent.Health = _playerScript.healthComponent.maxHealth;
            _playerScript.gems = 0;
            _playerScript.playerLives = _playerScript.playerLivesStartAmount;
        }
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
        
        public void LoadCheckpointBindData() {
            _playerScript.transform.position = DataInGameData.PlayerPosition;
        }

        [ContextMenu(nameof(SaveBindData))]
        public void SaveBindData() {
            DataInGameData.PlayerPosition = _playerScript.transform.position;
        }
        #endregion
    }
}