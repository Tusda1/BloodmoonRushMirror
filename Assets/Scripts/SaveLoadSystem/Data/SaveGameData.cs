using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using JetBrains.Annotations;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class SaveGameData : IGameData {
        [field: SerializeField] public string Name { get; set; }

        [SerializeField] public PlayerData playerData = new();
        
        public string currentSceneCollectionName;

        [SerializeReference] public List<LevelData> levelDatas = new();

        public LevelData GetCurrentLevelData() {
            return GetLevel(currentSceneCollectionName);
        }

        public void SetCurrentLevelData([CanBeNull] LevelData value) {
            if (value == null) {
                levelDatas.Remove(GetCurrentLevelData());
            }
            else {
                levelDatas.SetOrAddByIDInList(currentSceneCollectionName, () => value);
            }
        }

        public LevelData GetLevel(string id) => levelDatas.SingleOrDefault(x => x.ID == id);

        public object Clone() => this.DeepClone();
    }

    public static class SaveGameDataExtensions {
        public static T GetOrAddByIDInList<T>(this IList<T> saveGameDataList, string id, Func<T> generator) 
            where T : ISaveable, new() {
            var data = saveGameDataList.SingleOrDefault(x => x.ID == id);
            if (data != null) return data;

            data = generator();
            saveGameDataList.Add(data);
            return data;
        }

        public static void SetOrAddByIDInList<T>(this IList<T> saveGameDataList, string id, Func<T> generator) 
            where T : ISaveable, new() {
            var data = saveGameDataList.SingleOrDefault(x => x.ID == id);

            if (data == null) {
                data = generator();
                saveGameDataList.Add(data);
            }
            else {
                saveGameDataList.Remove(data);
                saveGameDataList.Add(generator());
            }
        }
    }
}