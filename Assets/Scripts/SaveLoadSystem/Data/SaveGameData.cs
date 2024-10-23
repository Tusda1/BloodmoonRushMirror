using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class SaveGameData : IGameData {
        [field: SerializeField] public string Name { get; set; }
        public DateTime LastSaveTime { get; set; }

        [SerializeField] public PlayerData playerData;

        public object Clone() {
            var clonedGameData = (SaveGameData)MemberwiseClone();
            
            clonedGameData.playerData = (PlayerData)playerData.Clone();
            
            return clonedGameData;
        }
    }
}