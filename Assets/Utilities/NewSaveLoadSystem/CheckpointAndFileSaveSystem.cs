using System.IO;
using System.Linq;
using ExtensionMethods;
using SaveLoadSystem.Data;
using SceneCollections;
using Utilities.DataBinding;
using VContainer;

public class CheckpointAndFileSaveSystem : ISaveLoadSystemManager {
    #region ---Fields---
    public IDataService<SaveGameData> DataService { get; private set; }
    public DataBinder DataBinder { get; private set; }

    public SaveGameData CurrentGameData { get; set; }

    #region ***---DI---***
    private DebugLoggerSO _debugLogger;
    private SaveSlots _saveSlots;
    private GameDataScrubGeneric<SaveGameData> _newGameData;
    private IProvider<SceneCollectionSO> _sceneCollectionProvider;
    #endregion
    #endregion

    #region ---Initialization---
    [Inject]
    public void Construct(IObjectResolver container) {
        _debugLogger = container.ResolveProviderID<DebugLoggerSO>("SaveLoadSystem");

        DataService = container.Resolve<IDataService<SaveGameData>>();
        DataBinder = container.Resolve<DataBinder>();

        _saveSlots = container.Resolve<SaveSlots>();
        _newGameData = container.Resolve<GameDataScrubGeneric<SaveGameData>>();

        _sceneCollectionProvider = container.Resolve<IProvider<SceneCollectionSO>>();
    }
    #endregion

    #region ---SaveSlots---
    public void UpdateSaveSlots() {
        LoadSaveSlotData(_saveSlots.saveSlot1Data);
        LoadSaveSlotData(_saveSlots.saveSlot2Data);
        LoadSaveSlotData(_saveSlots.saveSlot3Data);
    }

    private void LoadSaveSlotData(SaveSlotDataSO saveSlotData) {
        saveSlotData.Clear();

        var pathToSaveFile = DataService.GetPathToSaveFile(saveSlotData.saveSlotName);
        if (!File.Exists(pathToSaveFile)) {
            _debugLogger.Log(pathToSaveFile + " does not exist!");
            return;
        }

        var metaData = DataService.ReadFileMetadata(pathToSaveFile);
        saveSlotData.lastTimePlayed = metaData.LastWriteTime.ToString("HH:mm dd/MM/yyyy");
        saveSlotData.saveGameData = DataService.LoadByPath(pathToSaveFile);
        saveSlotData.currentLevel = saveSlotData.saveGameData.currentSceneCollectionName;
        saveSlotData.currentProgress = saveSlotData.saveGameData.levelDatas
            .Single(x => x.ID == saveSlotData.saveGameData.currentSceneCollectionName)
            .currentProgress;
    }

    /// <summary>
    /// Sets CurrentGameData to values of the save slot.
    /// </summary>
    public void SetCurrentGameDataFromSaveSlot(SaveSlotDataSO saveSlotData) {
        CurrentGameData = (SaveGameData)DataService.LoadByName(saveSlotData.saveSlotName).Clone();

        _debugLogger.Log($"Setting CurrentGameData to values of save slot: '{saveSlotData.saveSlotName}'");
    }

    /// <summary>
    /// Clears a save slot of all data.
    /// </summary>
    public void DeleteSaveSlotData(SaveSlotDataSO saveSlotData) {
        DataService.DeleteByName(saveSlotData.saveSlotName);
        saveSlotData.Clear();

        _debugLogger.Log($"Deleting {typeof(SaveGameData)} with name '{saveSlotData.saveSlotName}'");
    }
    #endregion

    #region ---Save---
    /// <summary>
    /// Calls on all bind-ed objects to 'SaveBindData'.
    /// </summary>
    public void SaveAllDataBinds() {
        foreach (var dataBind in DataBinder.GameDataBinds) {
            dataBind.SaveBindData();

            _debugLogger.Log("Updating DataBind");
        }
#if SAVESYSTEM_ACTIVE
        SaveToFile();
#endif
    }

    /// <summary>
    /// Method calls "SaveAllDataBinds" and saves the "_currentGameData" using the "DataService" to a file.
    /// </summary>
    public void SaveToFile() {
        DataService.SaveToFile(CurrentGameData);

        _debugLogger.Log($"Saving {typeof(SaveGameData)}");
    }
    #endregion

    #region ---Load---
    /// <summary>
    /// Calls on all bind-ed objects to 'LoadCheckpointBindData'.
    /// </summary>
    public void CheckpointLoad() {
        foreach (var dataBind in DataBinder.GameDataBinds) {
            dataBind.LoadCheckpointBindData();

            _debugLogger.Log("Loading CheckpointBindData");
        }
    }

    /// <summary>
    /// Calls on all bind-ed objects to 'LoadBindData'.
    /// </summary>
    public void SaveLoad() {
        foreach (var dataBind in DataBinder.GameDataBinds) {
            dataBind.LoadBindData();

            _debugLogger.Log("Loading DataBind");
        }
    }
    #endregion

    /// <summary>
    /// Sets CurrentGameData with "_newGameData" as a base template.
    /// </summary>
    public void SetCurrentGameDataFromTemplate(string name) {
        CurrentGameData = (SaveGameData)_newGameData.gameData.Clone();

        CurrentGameData.Name = name;
        var newGameDataName = _newGameData.gameData.Name;

        _debugLogger.Log($"Setting CurrentGameData with name: '{CurrentGameData.Name}' from base '{newGameDataName}'");
    }

    public void GameOverLogic() {
        // --- GetCurrentData --- \\
        var currentPlayerData = CurrentGameData.playerData;
        var currentLevelData = CurrentGameData.GetCurrentLevelData();

        // --- GetNewData --- \\
        var newGameData = _newGameData.gameData.DeepClone();
        var newPlayerData = newGameData.playerData;
        var newLevelData = newGameData.GetLevel(currentLevelData.ID);

        // --- SetCurrentPlayerData --- \\
        currentPlayerData.lives = newPlayerData.lives;
        currentPlayerData.health = newPlayerData.health;
        currentPlayerData.tempHealth = newPlayerData.tempHealth;

        // --- SetCurrentLevelData --- \\
        currentLevelData.enemyDatas = newLevelData.enemyDatas;
        currentLevelData.checkpointDatas = newLevelData.checkpointDatas;
        currentLevelData.parallaxDatas = newLevelData.parallaxDatas;
        currentLevelData.levelTimerData = newLevelData.levelTimerData;
        currentLevelData.currentProgress = newLevelData.currentProgress;
        currentLevelData.musicData = newLevelData.musicData;

        // --- SaveData --- \\
        SaveToFile();
    }

    public SceneCollectionSO GetCurrentSceneCollection() {
        return _sceneCollectionProvider.Get(CurrentGameData.currentSceneCollectionName);
    }
}