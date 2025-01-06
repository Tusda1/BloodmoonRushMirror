using System;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class SaveGameData : IGameData {
        [field: SerializeField] public string Name { get; set; }

        [SerializeField] public PlayerData playerData = new();

        [field: SerializeField] public int CurrentLevel { get; set; }
        [SerializeReference] public List<LevelData> levelDatas = new();

        public object Clone() => this.DeepClone();
    }

    public static class SaveGameDataExtensions {
        public static T GetOrAddByIdInList<T>(this IList<T> saveGameDataList, string id, T itemToAddIfDoesntExist) 
            where T : ISaveable, new() {
            var data = saveGameDataList.FirstOrDefault(x => x.ID == id);

            if (data != null) return data;

            saveGameDataList.Add(itemToAddIfDoesntExist);

            return itemToAddIfDoesntExist;
        }
    }
}