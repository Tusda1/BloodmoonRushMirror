using System.Collections;
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
        private CameraEffects _cameraEffects;
        private GameDataScrubGeneric<SaveGameData> _newGameData;
        #endregion

        #region ---Initialization---
        [Inject]
        public void Construct(IObjectResolver container) {
#if SAVESYSTEM_ACTIVE
            _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
#if UNITY_EDITOR
            _newGameData = container.Resolve<GameDataScrubGeneric<SaveGameData>>();

            if (_saveLoadSystemManager.CurrentGameData == null) {
                _saveLoadSystemManager.SetCurrentGameDataFromTemplate("NOSAVESYSTEM");
            }
#endif
#endif
            _playerScript = container.Resolve<PlayerScript>();
            _cameraEffects = container.Resolve<CameraEffects>();
        }

        protected void Start() {
            Bind();
#if SAVESYSTEM_ACTIVE
#if UNITY_EDITOR
            if (SceneCollectionManager.CurrentSceneCollection == null) return;
#endif
            // if (_saveLoadSystemManager.CurrentGameData == null) return;
            
            _saveLoadSystemManager.SaveLoad();
            _saveLoadSystemManager.SaveAllDataBinds();
#else
#if UNITY_EDITOR
            if (SceneCollectionManager.CurrentSceneCollection == null) return;
#endif
            _playerScript.transform.position = PlayerPosition;
            _playerScript.healthComponent.Health = _playerScript.healthComponent.maxHealth;
            _playerScript.gems = 0;
            _playerScript.playerLives = _playerScript.playerLivesStartAmount;
#endif
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
#endif

#if SAVESYSTEM_ACTIVE && (UNITY_EDITOR || DEVELOPMENT_BUILD)
        private const string SaveSystemMenu = "SaveSystemManager/";
        
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