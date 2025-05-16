using System;
using Player;
using SaveLoadSystem.Data;
using SceneCollections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.DataBinding;
using VContainer;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using ExtensionMethods;
using System.Collections;
using Steamworks;
#endif

namespace SaveLoadSystem {
    public partial class LevelDataManager : MonoBehaviour, IBind<LevelData> {
        #region ---Fields---
        [SerializeField] private bool loadPlayerPositionOnStart;
        [SerializeField] private SceneCollectionSO otherLevel;
        
        private ISaveLoadSystemManager _saveLoadSystemManager;
        private PlayerScript _playerScript;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private GameDataScrubGeneric<SaveGameData> _newGameData;
        private CameraEffects _cameraEffects;

        private const string SaveSystemMenu = "SaveSystemManager/";
#endif
        [HideInInspector] public bool startedInLevel;
        #endregion

        #region ---Initialization---
        [Inject]
        public void Construct(IObjectResolver container) {
            _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
            _playerScript = container.Resolve<PlayerScript>();
#if UNITY_EDITOR
            if (_saveLoadSystemManager.CurrentGameData == null) {
                startedInLevel = true;
                _saveLoadSystemManager.SetCurrentGameDataFromTemplate("DATAMANAGER");
            }
#endif
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _newGameData = container.Resolve<GameDataScrubGeneric<SaveGameData>>();
            _cameraEffects = container.Resolve<CameraEffects>();
#endif
        }

        protected void Start() {
            Bind();
#if UNITY_EDITOR
            if (startedInLevel && !loadPlayerPositionOnStart) {
                DataInGameData.PlayerPosition = _playerScript.transform.position;
            }
#endif
            _saveLoadSystemManager.SaveLoad();
            
            SceneManager.sceneUnloaded += UnbindAll;
        }

        private void OnDestroy() {
            
            SceneManager.sceneUnloaded -= UnbindAll;
        }
        #endregion

#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
            if (Input.GetKeyDown(KeyCode.F4) && otherLevel != null) {
                _saveLoadSystemManager.CurrentGameData = null;
                _saveLoadSystemManager.SetCurrentGameDataFromTemplate("NO_SAVE_SYSTEM");
                _cameraEffects.FadeLoadScene(otherLevel);
            }

            if (Input.GetKeyDown(KeyCode.F10)) {
                SteamUserStats.ClearAchievement(IAchievementManager.AchievementID_First);
                SteamUserStats.ClearAchievement(IAchievementManager.AchievementID_EntranceHall);
                SteamUserStats.ClearAchievement(IAchievementManager.AchievementID_TheDungeons);
                SteamUserStats.ClearAchievement(IAchievementManager.AchievementID_TheBoiler);
                SteamUserStats.ClearAchievement(IAchievementManager.AchievementID_CastleMain);
                SteamUserStats.ClearAchievement(IAchievementManager.AchievementID_ToweringHeights);
                SteamUserStats.ClearAchievement(IAchievementManager.AchievementID_DraculasOffice);
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
            _newGameData.gameData.levelDatas.SetOrAddByIDInList(ID, ClonedData);
            return;

            LevelData ClonedData() => DataInGameData.DeepClone();
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