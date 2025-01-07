using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem {
    public class SaveLoadSystemManager : SaveLoadSystemBase<SaveLoadSystemManager, SaveGameData> {
        protected override void Awake() {
            base.Awake();

            DataService = new FileDataService<SaveGameData>();
        }

        private IEnumerator Start() {
            yield return null;
            
            SaveAllDataBinds();
            LoadAllDataBinds();
        }

        [ContextMenu("Runtime/" + nameof(SaveToFile))]
        public void SaveToFile() {
            SaveGame();
        }

        [ContextMenu("Runtime/" + nameof(LoadMostRecentSaveFile))]
        public void LoadMostRecentSaveFile() {
            var mostRecentSaveFile = DataService.GetMostRecentSaveFile();

            if (mostRecentSaveFile == null) {
                NewGame();
            }
            else {
                LoadSave(mostRecentSaveFile.Name);
            }
        }

        [ContextMenu(nameof(SaveBindings))]
        public void SaveBindings() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                SetDataBindsInEditor();
            }
#endif

            SaveAllDataBinds();
        }

        [ContextMenu(nameof(LoadDataBinds))]
        public void LoadDataBinds() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                SetDataBindsInEditor();
            }
#endif

            LoadAllDataBinds();
        }

#if UNITY_EDITOR
        [ContextMenu(nameof(WriteToNewGameDataSO))]
        public void WriteToNewGameDataSO() {
            WriteToNewGameData();
        }
#endif
        [ContextMenu(nameof(LoadFromNewGameDataSO))]
        public void LoadFromNewGameDataSO() {
            gameData = (SaveGameData)newGameData.gameData.Clone();
        }

        [ContextMenu(nameof(ResetGameData))]
        public void ResetGameData() {
            gameData = new SaveGameData();
        }

#if UNITY_EDITOR
        private void SetDataBindsInEditor() {
            if (Application.isPlaying) return;

            try {
                var dataBinds = GetAllDataBindsEditMode().ToArray();
                if (dataBinds.Length == 0) {
                    Debug.LogWarning("No data binds found to save.");
                    return;
                }

                debugLogger.Log($"{dataBinds.Length} data binds saved successfully.");
            }
            catch (Exception ex) {
                Debug.LogError($"Error while saving data binds: {ex.Message}");
            }
        }

        private static IEnumerable<IDataBind> GetAllDataBindsEditMode() {
            var allDataBinds = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IDataBind>();

            foreach (var dataBind in allDataBinds) {
                dataBind.Bind();
                yield return dataBind;
            }
        }

        [ContextMenu(nameof(OpenFileLocation))]
        private void OpenFileLocation() {
            DataService ??= new FileDataService<SaveGameData>();
            System.Diagnostics.Process.Start(DataService.DataPath);
        }
#endif
    }
}