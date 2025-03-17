using Utilities.DataBinding;

public interface ISaveLoadSystemManagerGeneric<TGameData> where TGameData : IGameData {
    IDataService<TGameData> DataService { get; }
    DataBinder DataBinder { get; }
    TGameData CurrentGameData { get; set; }

    void UpdateSaveSlots();

    void SaveAllDataBinds();
    void SaveToFile();

    void CheckpointLoad();
    void SaveLoad();

    void SetCurrentGameDataFromTemplate(string gameDataName);
    void SetCurrentGameDataFromSaveSlot(SaveSlotDataSO saveSlotData);
    void DeleteSaveSlotData(SaveSlotDataSO saveSlotData);
}