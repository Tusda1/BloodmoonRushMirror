using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [Header("DebugLogger")] [SerializeField, Attributes.Required]
        private DebugLoggerSO debugLogger;

        public readonly List<IDataBind> GameDataBinds = new();

        public IDataService<TGameData> DataService { get; protected set; }

        /// <summary>
        /// Base "NewGame" method creates a new save with the "newGameData" as a base reference.
        /// </summary>
        public virtual void NewGame() {
            gameData = (TGameData)newGameData.gameData.Clone();

            var saveNumber = 0;

            while (File.Exists(gameData.name + saveNumber)) {
                saveNumber++;
            }

            gameData.name += saveNumber;

            debugLogger.Log($"Creating new game data with name '{gameData.name}' with values from '{newGameData.gameData.name}'");
        }

        /// <summary>
        /// Updates all dataBinds for the IGameData type of this SaveLoadSystem.
        /// </summary>
        public virtual void SaveAllDataBinds() {
            foreach (var dataBind in GameDataBinds) {
                dataBind.SaveBindData();

                debugLogger.Log("Updating DataBind");
            }
        }

        /// <summary>
        /// Base "SaveGame" method calls "UpdateAllDataBinds"  and saves the "gameData" using the "DataService".
        /// </summary>
        public virtual void SaveGame() {
            debugLogger.Log($"Saving {typeof(TGameData)}");
            
            SaveAllDataBinds();

            DataService.Save(gameData);
        }

        /// <summary>
        /// Base "LoadSave" method loads a save using the "DataService" and sets "gameData" to that save.
        /// </summary>
        public virtual void LoadSave(string saveName) {
            debugLogger.Log($"Loading {typeof(TGameData)} with name '{saveName}'");
            
            gameData = DataService.LoadByName(saveName);
        }
        
        public virtual void LoadAllDataBinds() {
            foreach (var dataBind in GameDataBinds) {
                dataBind.LoadBindData();

                debugLogger.Log("Loading DataBind");
            }
        }

        /// <summary>
        /// Deletes a save by the given name.
        /// </summary>
        public void DeleteSave(string saveName) {
            DataService.DeleteByName(saveName);

            debugLogger.Log($"Deleting  {typeof(TGameData)} with name '{saveName}'");
        }

        public void WriteToNewGameData() {
            debugLogger.Log($"Writing default {typeof(TGameData)} to {newGameData.name}");
            
            newGameData.gameData = (TGameData)gameData.Clone();
        }

        /// <summary>
        /// Binds "TBind" to the "TData". Finds the first object of the "TBind" type.
        /// </summary>
        public virtual void Bind<T, TData>(TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable {
            var bind = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if (bind == null) return;

            Bind(bind, data);
        }

        /// <summary>
        /// Binds "TBind" to the "TData". "TBind" object is given explicitly.
        /// </summary>
        public virtual void Bind<TBind, TData>(TBind bind, TData data)
            where TBind : MonoBehaviour, IBind<TData> where TData : ISaveable {
            GameDataBinds.Add(bind);
            bind.saveSystemData = data;
            debugLogger.Log($"{typeof(TBind)} object bind-ed to {typeof(TData)}");
        }

        public virtual void UnBind<TBind, TData>(TBind bind, TData data)
            where TBind : MonoBehaviour, IBind<TData> where TData : ISaveable {
            GameDataBinds.Remove(bind);
            bind.saveSystemData = default;
            debugLogger.Log($"{typeof(TBind)} object un-bind-ed from {typeof(TData)}");
        }
    }
}