using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.DataBinding;
using SceneCollections;

namespace SaveLoadSystem.Data {
    [Serializable] public class LevelData : ISaveable {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public SceneCollectionSO SceneCollection { get; set; }
        [field: SerializeField] public Vector3 LevelStartPoint { get; set; }
        [field: SerializeField] public CurrentCheckpointData Checkpoint { get; set; }
        
        [SerializeReference] public List<EnemyData> enemyDatas = new();
    }
}