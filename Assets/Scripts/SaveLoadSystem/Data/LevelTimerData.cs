using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class LevelTimerData : ISaveable {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public float Time { get; set; }
    }
}