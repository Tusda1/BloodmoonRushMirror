using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.SOGenerators {
    [CreateAssetMenu(fileName = "NewGameData", menuName = "SaveLoadSystem/Create NewGameData")]
    public class NewGameData : GameDataScrubGeneric<SaveGameData> {
    }
}