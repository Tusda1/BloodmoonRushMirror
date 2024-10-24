using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    /// <summary>
    /// 'Id' must be serialized with '[field: SerializeField]' to be visible in the inspector
    /// </summary>
    [Serializable] public class PlayerData : ISaveable { 
        [field: SerializeField] public string Id { get; set; }
        public int health;
        public int lives;
        public int gems;
        
        public object Clone() {
            var clone = (PlayerData)MemberwiseClone();
            return clone;
        }
    }
}