using UnityEngine;

namespace Utilities.DataBinding {
    public abstract class GameDataScrubGeneric<TGameData> : ScriptableObject {
        [SerializeField] public TGameData gameData;
    }
}