using SaveLoadSystem.Data;
using Utilities.DataBinding;

namespace SaveLoadSystem {
    public class SaveLoadSystem : SaveLoadSystemBase<SaveLoadSystem, SaveGameData> {
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