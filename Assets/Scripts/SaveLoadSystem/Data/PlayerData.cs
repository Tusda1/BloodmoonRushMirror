using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class PlayerData : ISaveable {
        public string Id { get; set; }
        public Vector3 position;
        public int health;
        public int gems;
        public int lives;
        
        public object Clone() {
            var clone = (PlayerData)MemberwiseClone();
            return clone;
        }
    }
}