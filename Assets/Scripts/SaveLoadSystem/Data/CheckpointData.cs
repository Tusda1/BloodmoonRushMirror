using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class CheckpointData : ISaveable {
        public string name;
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public bool HasBeenTriggered { get; set; }
        [field: SerializeField] public Vector3 Position { get; set; }
    }
}