using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.DataBinding;
using SceneCollections;

namespace SaveLoadSystem.Data {
    [Serializable] public class LevelData : ISaveable {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public SceneCollectionSO SceneCollection { get; set; }
        [field: SerializeField] public Vector3 PlayerPosition { get; set; }
        [field: SerializeField] public CheckpointData CurrentCheckpointData { get; set; }
        [SerializeReference] public List<CheckpointData> checkpointDatas = new();
        [SerializeReference] public List<TriggerData> triggerDatas = new();
        [SerializeReference] public List<ParallaxData> parallaxDatas = new();
        
        [SerializeReference] public List<EnemyData> enemyDatas = new();
        
        [field: SerializeField] public MusicData MusicData { get; set; }
    }
}