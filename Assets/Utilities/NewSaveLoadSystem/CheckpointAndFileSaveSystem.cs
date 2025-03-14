using System.IO;
using SaveLoadSystem.Data;
using UnityEngine;
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
    }
    #endregion

    #region ---SaveSlots---
    public void UpdateSaveSlotSO() {
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
    }

    /// <summary>
    /// Method calls "SaveAllDataBinds" and saves the "_currentGameData" using the "DataService" to a file.
    /// </summary>
    public void SaveToFile() {
        SaveAllDataBinds();

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

        _debugLogger.Log($"Creating new game data with name '{CurrentGameData.Name}' with values from '{_newGameData.gameData.Name}'");
    }
}