using Utilities.DataBinding;

public interface ISaveLoadSystemManagerGeneric<TGameData> where TGameData : IGameData {
    IDataService<TGameData> DataService { get; }
    DataBinder DataBinder { get; }
    TGameData CurrentGameData { get; }

    void UpdateSaveSlotSO();

    void SaveAllDataBinds();
    void SaveToFile();

    void CheckpointLoad();
    void SaveLoad();

    void NewSaveSlotDataData(SaveSlotDataSO saveSlotData);
    void SetCurrentGameDataFromSaveSlot(SaveSlotDataSO saveSlotData);
    void DeleteSaveSlotData(SaveSlotDataSO saveSlotData);
}