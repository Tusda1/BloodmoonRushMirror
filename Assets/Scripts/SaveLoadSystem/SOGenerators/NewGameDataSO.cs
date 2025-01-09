using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.SOGenerators {
    [CreateAssetMenu(fileName = nameof(NewGameDataSO), menuName = "SaveLoadSystem/Create " + nameof(NewGameDataSO))]
    public class NewGameDataSO : GameDataScrubGeneric<SaveGameData> {
        [ContextMenu(nameof(ResetSO))]
        private void ResetSO() {
            gameData = null;
        }
    }
}