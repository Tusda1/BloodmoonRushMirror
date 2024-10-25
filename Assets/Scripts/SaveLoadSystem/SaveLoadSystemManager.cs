using System.Collections;
using SaveLoadSystem.Data;
using UnityEngine;
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

        private IEnumerator Start() {
            yield return null;
            
            LoadAllDataBinds();
        }

        [ContextMenu("WriteToNewGameData()")]
        public void WriteToNewGameDataCall() => WriteToNewGameData();

        [ContextMenu("SaveToFile")]
        public void SaveToFile() => SaveGame();
    }
}