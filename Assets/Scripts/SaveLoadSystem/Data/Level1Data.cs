using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class Level1Data : IGameData {
        [field: SerializeField] public string Name { get; set; }
        public DateTime LastSaveTime { get; set; }
        
        public object Clone() {
            var clonedGameData = (SaveGameData)MemberwiseClone();
            
            return clonedGameData;
        }
    }
}