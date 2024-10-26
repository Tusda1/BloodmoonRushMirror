using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class EnemyData : ISaveable {
        public string name;
        [field: SerializeField] public string id { get; set; }
        public bool isAlive;
        public int health;
        public int gemsContaining;
        public Vector3 position;
    }
}