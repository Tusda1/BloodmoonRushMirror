using System;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class MusicData : ISaveable {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string MusicID { get; set; }
        [field: SerializeField] public float CalmnessParameter { get; set; }
    }
}