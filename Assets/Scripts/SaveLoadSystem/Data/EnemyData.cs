using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class EnemyData : ISaveable {
        [field: SerializeField] public string id { get; set; }
        public bool isAlive;
        public int health;
        public int gemsContaining;
        
        public object Clone() {
            var clonedGameData = (EnemyData)MemberwiseClone();
            return clonedGameData;
        }
    }
}