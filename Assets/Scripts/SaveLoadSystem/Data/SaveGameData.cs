using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class SaveGameData : IGameData {
        [field: SerializeField] public string name { get; set; }

        [SerializeField] public PlayerData playerData;

        [SerializeReference] public List<LevelData> levelData = new();

        [SerializeReference] public List<EnemyData> enemies = new();

        public object Clone() => this.DeepClone();
    }

    public static class SaveGameDataExtensions {
        public static T GetOrAddByIdInList<T>(this IList<T> saveGameDataList, string id, T itemToAddIfDoesntExist)
            where T : ISaveable, new() {
            var data = saveGameDataList.FirstOrDefault(x => x.id == id);

            if (data != null) return data;

            saveGameDataList.Add(itemToAddIfDoesntExist);

            return itemToAddIfDoesntExist;
        }
    }
}