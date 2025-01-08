using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class TriggerData : ISaveable {
        public string name;
        [field: SerializeField] public string ID { get; set; }
        public bool hasBeenTriggered;
    }
}