using System.Collections.Generic;
using System.IO;
using System.Linq;
using Attributes;
using UnityEngine;

namespace Utilities.DataBinding {
    /// <summary>
    /// Abstract base class for managing save and load operations.
    /// </summary>
    public abstract class SaveLoadSystemBase<TComponent, TGameData> : PersistentSingletonLazyLoad<TComponent>
        where TGameData : class, IGameData where TComponent : Component {
        public TGameData gameData;

        [Header("New Save Default Values")] [SerializeField]
        public GameDataScrubGeneric<TGameData> newGameData;

        [Header("DebugLogger")] [SerializeField, Required]
        private DebugLoggerSO debugLogger;

        public readonly List<IDataBind> GameDataBinds = new();

        public IDataService<TGameData> DataService { get; protected set; }

        /// <summary>
        /// Base "NewGame" method creates a new save with the "newGameData" as a base reference.
        /// </summary>
        public virtual void NewGame() {
            gameData = (TGameData)newGameData.gameData.Clone();

            var saveNumber = 0;

            while (File.Exists(gameData.Name + saveNumber)) {
                saveNumber++;
            }

            gameData.Name += saveNumber;

            debugLogger.Log($"Creating new game data with name '{gameData.Name}' with values from '{newGameData.gameData.Name}'");
        }

        /// <summary>
        /// Updates all dataBinds for the IGameData type of this SaveLoadSystem.
        /// </summary>
        public virtual void UpdateAllDataBinds() {
            foreach (var dataBind in GameDataBinds) {
                dataBind.SaveBindData();

                debugLogger.Log("Updating DataBind");
            }
        }

        /// <summary>
        /// Base "SaveGame" method calls "UpdateAllDataBinds"  and saves the "gameData" using the "DataService".
        /// </summary>
        public virtual void SaveGame() {
            UpdateAllDataBinds();

            DataService.Save(gameData);

            debugLogger.Log($"Saving {typeof(TGameData)}");
        }

        /// <summary>
        /// Base "LoadSave" method loads a save using the "DataService" and sets "gameData" to that save.
        /// </summary>
        public virtual void LoadSave(string saveName) {
            gameData = DataService.LoadByName(saveName);

            debugLogger.Log($"Loading {typeof(TGameData)} with name '{saveName}'");
        }

        /// <summary>
        /// Deletes a save by the given name.
        /// </summary>
        public void DeleteSave(string saveName) {
            DataService.DeleteByName(saveName);

            debugLogger.Log($"Deleting  {typeof(TGameData)} with name '{saveName}'");
        }

        [ContextMenu("Write To New Save Default Values")]
        public void WriteToNewGameData() {
            newGameData.gameData = (TGameData)gameData.Clone();

            debugLogger.Log($"Writing default {typeof(TGameData)} to {newGameData.name}");
        }

        /// <summary>
        /// Binds "TBind" to the "TData". Finds the first object of the "TBind" type.
        /// </summary>
        public virtual void Bind<T, TData>(TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable {
            var bind = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if (bind == null) return;

            GameDataBinds.Add(bind);
            bind.Bind(data);
        }

        /// <summary>
        /// Binds "TBind" to the "TData". "TBind" object is given explicitly.
        /// </summary>
        public virtual void Bind<TBind, TData>(TBind bind, TData data)
            where TBind : MonoBehaviour, IBind<TData> where TData : ISaveable {
            GameDataBinds.Add(bind);
            bind.Bind(data);
        }

        public virtual void UnBind<TBind, TData>(TBind bind, TData data)
            where TBind : MonoBehaviour, IBind<TData> where TData : ISaveable {
            GameDataBinds.Remove(bind);
            bind.UnBind(data);
        }
    }
}