using System;
using System.Collections;
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

        private IList CloneListByType(IList sourceList, Type targetType) {
            var clonedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType));
            foreach (var item in sourceList) {
                clonedList.Add(DeepClone((ISaveable)item));
            }
            return clonedList;
        }

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
                    case IList cloneableList when fieldInfo.FieldType.IsGenericType:
                        var targetType = fieldInfo.FieldType.GetGenericArguments()[0];
                        clonedValue = CloneListByType(cloneableList, targetType);
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
        public static T GetOrAddByIdInList<T>(this IList<T> saveGameDataList, string id, T itemToAddIfDoesntExist)
            where T : ISaveable, new() {
            var data = saveGameDataList.FirstOrDefault(x => x.id == id);

            if (data != null) return data;

            saveGameDataList.Add(itemToAddIfDoesntExist);

            return itemToAddIfDoesntExist;
        }
    }
}