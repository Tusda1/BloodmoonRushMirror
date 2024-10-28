using System;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem {
    public class LevelDataManager : Singleton<LevelDataManager>, IBind<LevelData> {
        #region ---Fields---
        #region ***---GameManagers---***
        private static SaveLoadSystemManager SaveLoadSystemManager => SaveLoadSystemManager.Instance;
        #endregion

        #region ***---DataBindingVariables---***
        [field: Header("Data Binding")]
        [field: SerializeField]
        public string id { get; set; } = Guid.NewGuid().ToString();

        [field: SerializeField] public LevelData saveSystemData { get; set; }

        public LevelData dataInGameData => SaveLoadSystemManager.gameData.levelDatas.GetOrAddByIdInList(id,
            new LevelData {
                id = id
            });
        #endregion
        #endregion

        private void Start() {
            Bind();
        }

        #region ---DataBindingMethods---

        public void Bind() {
            SaveLoadSystemManager.Bind(this, dataInGameData);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (gameObject.activeInHierarchy && string.IsNullOrEmpty(id)) {
                id = Guid.NewGuid().ToString();
                Debug.Log($"Setting new guid for {nameof(LevelDataManager)}");
            }
        }
#endif

        [ContextMenu(nameof(LoadBindData))]
        public void LoadBindData() {
        }

        [ContextMenu(nameof(SaveBindData))]
        public void SaveBindData() {
        }
        #endregion
    }
}