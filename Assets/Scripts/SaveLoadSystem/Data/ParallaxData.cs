using System;
using Unity.Cinemachine;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class ParallaxData : ISaveable {
        [field: SerializeField] public string ID { get; set; }
        public CinemachineCamera Camera { get; set; }
        [field: SerializeField] public Vector3 CameraPosition { get; set; }
        [field: SerializeField] public Vector3[] Positions { get; set; }
    }
}