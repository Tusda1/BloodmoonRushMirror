using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    /// <summary>
    /// 'id' must be serialized with '[field: SerializeField]' to be visible in the inspector
    /// </summary>
    [Serializable] public class PlayerData : ISaveable {
        [field: SerializeField] public string id { get; set; }
        public int health;
        public int lives;
        public int gems;

        public object Clone() {
            var clone = (PlayerData)MemberwiseClone();
            return clone;
        }
    }
}