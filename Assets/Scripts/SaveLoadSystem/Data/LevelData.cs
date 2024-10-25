using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class LevelData : ISaveable {
        [field: SerializeField] public string id { get; set; }

        public object Clone() {
            var clonedGameData = (LevelData)MemberwiseClone();
            return clonedGameData;
        }
    }
}