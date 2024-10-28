using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class PlayerData : ISaveable {
        [field: SerializeField] public string ID { get; set; }
        public int health;
        public int lives;
        public int gems;
    }
}