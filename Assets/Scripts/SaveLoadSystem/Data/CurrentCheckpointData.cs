using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class CurrentCheckpointData : ISaveable {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public Vector3 Position { get; set; }
    }
}