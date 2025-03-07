using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class PlayerData : ISaveable {
        [field: SerializeField] public string ID { get; set; }

        public int health;
        public int tempHealth;
        public int lives;
        public int gems;
    }
}