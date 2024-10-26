using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem.Data {
    [Serializable] public class SaveGameData : IGameData {
        [field: SerializeField] public string name { get; set; }

        [SerializeField] public PlayerData playerData;

        [SerializeReference] public List<LevelData> levelData = new();

        [SerializeReference] public List<EnemyData> enemies = new();

        public object Clone() => DeepClone(this);

        public object DeepClone(object obj) {
            if (obj == null) return null;

            var clonedObj = obj.GetType()
                .GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(obj, null);

            var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var fieldInfo in fields) {
                var value = fieldInfo.GetValue(obj);
                if (value == null) continue;

                object clonedValue;
                switch (value) {
                    case ISaveable cloneable:
                        clonedValue = DeepClone(cloneable);
                        break;
                    case IEnumerable<ISaveable> cloneableCollection:
                        var clonedList = Activator.CreateInstance(value.GetType()) as IList<ISaveable>;
                        foreach (var item in cloneableCollection) {
                            clonedList?.Add((ISaveable)DeepClone(item));
                        }

                        clonedValue = clonedList;
                        break;
                    default:
                        clonedValue = value;
                        break;
                }

                fieldInfo.SetValue(clonedObj, clonedValue);
            }

            return clonedObj;
        }
    }

    public static class SaveGameDataExtensions {
        public static T GetOrAddByIdInList<T>(this IList<T> saveGameDataList, string id, T itemToAddIfDoesntExist) where T : ISaveable, new() {
            var data = saveGameDataList.FirstOrDefault(x => x.id == id);

            if (data != null) return data;
            
            saveGameDataList.Add(itemToAddIfDoesntExist);

            return itemToAddIfDoesntExist;
        }
    }
}