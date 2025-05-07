using SaveLoadSystem.Data;

public interface ISaveLoadSystemManager : ISaveLoadSystemManagerGeneric<SaveGameData> {
    public bool ResetLevelOnGameOver { get; set; }
}