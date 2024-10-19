using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities.DataBinding {
    /// <summary>
    /// Abstract base class for managing save and load operations.
    /// </summary>
    public abstract class SaveLoadSystemBase<TComponent, TGameData> : PersistentSingletonLazyLoad<TComponent>
        where TGameData : class, IGameData where TComponent : Component {
        public TGameData gameData;
        [SerializeField] public GameDataScrubGeneric<TGameData> newGameData;

        

        public readonly List<IDataBind> GameDataBinds = new();

        public bool canSave = true;

        public IDataService<TGameData> DataService { get; protected set; }

        /// <summary>
        /// Base "NewGame" method creates a new save with the "newGameData" as a base reference.
        /// </summary>
        public virtual void NewGame() {
            gameData = (TGameData)newGameData.gameData.Clone();

            var saveNumber = DataService.GetSaveCount() + 1;
            gameData.Name += saveNumber;
        }
        
        /// <summary>
        /// Updates all dataBinds for the IGameData type of this SaveLoadSystem.
        /// </summary>
        public virtual void UpdateAllDataBinds() {
            foreach (var dataBind in GameDataBinds) {
                dataBind.SaveBindData();
            }
        }

        /// <summary>
        /// Base "SaveGame" method calls "UpdateAllDataBinds"  and saves the "gameData" using the "DataService".
        /// </summary>
        public virtual void SaveGame() {
            UpdateAllDataBinds();

            DataService.Save(gameData);
        }

        /// <summary>
        /// Base "LoadSave" method loads a save using the "DataService" and sets "gameData" to that save.
        /// </summary>
        public virtual void LoadSave(string saveName) => gameData = DataService.Load(saveName);

        /// <summary>
        /// Loads the current "gameData" save.
        /// </summary>
        public virtual void LoadCurrentSave() => LoadSave(gameData.Name);

        /// <summary>
        /// Deletes a save by the given name.
        /// </summary>
        public void DeleteSave(string saveName) => DataService.Delete(saveName);

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
        public virtual void Bind<TBind, TData>(TBind bind, TData data) where TBind : MonoBehaviour, IBind<TData>
            where TData : ISaveable {
            GameDataBinds.Add(bind);
            bind.Bind(data);
        }

        public virtual void UnBind<TBind, TData>(TBind bind, TData data) where TBind : MonoBehaviour, IBind<TData>
            where TData : ISaveable {
            GameDataBinds.Remove(bind);
            bind.UnBind(data);
        }
    }
}