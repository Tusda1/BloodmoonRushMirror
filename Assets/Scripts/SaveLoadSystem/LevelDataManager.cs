using Player;
using SaveLoadSystem.Data;
using SceneCollections;
using UnityEngine;
using Utilities.DataBinding;
using VContainer;

namespace SaveLoadSystem {
    public partial class LevelDataManager : MonoBehaviour, IBind<LevelData> {
        #region ---Fields---
        [SerializeField] private SceneCollectionSO otherLevel;
        
        private ISaveLoadSystemManager _saveLoadSystemManager;
        private PlayerScript _playerScript;
        private GameDataScrubGeneric<SaveGameData> _newGameData;
#if SAVESYSTEM_ACTIVE && (UNITY_EDITOR || DEVELOPMENT_BUILD)
        private CameraEffects _cameraEffects;

        private const string SaveSystemMenu = "SaveSystemManager/";
#endif
        
        [HideInInspector] public bool startedInLevel;
        #endregion

        #region ---Initialization---
        [Inject]
        public void Construct(IObjectResolver container) {
            _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
#if UNITY_EDITOR
            if (_saveLoadSystemManager.CurrentGameData == null) {
                startedInLevel = true;
                _saveLoadSystemManager.SetCurrentGameDataFromTemplate("DATAMANAGER");
            }
#endif
            _playerScript = container.Resolve<PlayerScript>();
#if SAVESYSTEM_ACTIVE && (UNITY_EDITOR || DEVELOPMENT_BUILD)
            _cameraEffects = container.Resolve<CameraEffects>();
#endif
        }

        protected void Start() {
            Bind();
#if UNITY_EDITOR
            if (startedInLevel) {
                DataInGameData.PlayerPosition = _playerScript.transform.position;
            }
#endif
            _saveLoadSystemManager.SaveLoad();
        }
        #endregion

#if SAVESYSTEM_ACTIVE && (UNITY_EDITOR || DEVELOPMENT_BUILD)
        private void Update() {
            if (Input.GetKeyDown(KeyCode.F5)) {
                SaveLoadSystemManager_SaveAllDataBinds();
            }
            if (Input.GetKeyDown(KeyCode.F6)) {
                StartCoroutine(LoadCheckpoint());
            }
            if (Input.GetKeyDown(KeyCode.F7)) {
                SaveLoadSystemManager_SaveLoad();
            }
            if (Input.GetKeyDown(KeyCode.F4)) {
                _saveLoadSystemManager.CurrentGameData = null;
                _saveLoadSystemManager.SetCurrentGameDataFromTemplate("NOSAVESYSTEM");
                _cameraEffects.FadeLoadScene(otherLevel);
            }
        }

        private IEnumerator LoadCheckpoint() {
            _saveLoadSystemManager.CheckpointLoad();
            yield return null;
            yield return null;
            yield return null;
            _saveLoadSystemManager.CheckpointLoad();
        }
        
        [ContextMenu(SaveSystemMenu + nameof(WriteDataToNewGameData))]
        public void WriteDataToNewGameData() {
            _newGameData.gameData.levelDatas.SetOrAddByIDInList(ID, LevelDataGenerator);
        }
        
        [ContextMenu(SaveSystemMenu + nameof(SaveLoadSystemManager_SaveLoad))]
        public void SaveLoadSystemManager_CheckpointLoad() {
            _saveLoadSystemManager.CheckpointLoad();
        }
        
        [ContextMenu(SaveSystemMenu + nameof(SaveLoadSystemManager_SaveLoad))]
        public void SaveLoadSystemManager_SaveLoad() {
            _saveLoadSystemManager.SaveLoad();
        }
        
        [ContextMenu(SaveSystemMenu + nameof(SaveLoadSystemManager_SaveAllDataBinds))]
        public void SaveLoadSystemManager_SaveAllDataBinds() {
            _saveLoadSystemManager.SaveAllDataBinds();
        }
#endif
    }
}