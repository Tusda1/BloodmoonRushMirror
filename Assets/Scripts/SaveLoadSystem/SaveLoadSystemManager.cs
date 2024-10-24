using SaveLoadSystem.Data;
using Utilities.DataBinding;

namespace SaveLoadSystem {
    public class SaveLoadSystemManager : SaveLoadSystemBase<SaveLoadSystemManager, SaveGameData> {
        protected override void Awake() {
            base.Awake();

            DataService = new FileDataService<SaveGameData>();

            var mostRecentSaveFile = DataService.GetMostRecentSaveFile();

            if (mostRecentSaveFile == null) {
                NewGame();
            }
            else {
                LoadSave(mostRecentSaveFile.Name);
            }
        }
    }
}