using SaveLoadSystem.Data;

public partial class SteamSpeedrunTimer {
    #region ---DataBindingVariables---
    public string ID { get; set; }
    public LevelTimerData SaveSystemData { get; set; }
    public LevelTimerData DataInGameData => _levelDataManager.DataInGameData.levelTimerData;
    #endregion

    #region ---DataBindingMethods---
    public void Bind() {
        _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
    }

    public void Unbind() {
        _saveLoadSystemManager.DataBinder.UnBind(this, DataInGameData);
    }

    public void LoadBindData() {
        _time = DataInGameData.Time;
    }

    public void SaveBindData() {
        DataInGameData.ID = _leaderboardID;
        DataInGameData.Time = _time;
    }
    #endregion
}