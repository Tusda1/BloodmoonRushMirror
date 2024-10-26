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

            dataService = new FileDataService<SaveGameData>();
        }

        private IEnumerator Start() {
            yield return null;

            LoadAllDataBinds();
        }

        [ContextMenu("Runtime/SaveToFile()")]
        public void SaveToFile() {
            SaveGame();
        }

        [ContextMenu("Runtime/LoadMostRecentSaveFile()")]
        public void LoadMostRecentSaveFile() {
            var mostRecentSaveFile = dataService.GetMostRecentSaveFile();

            if (mostRecentSaveFile == null) {
                NewGame();
            }
            else {
                LoadSave(mostRecentSaveFile.Name);
            }
        }

        [ContextMenu("SaveAllDataBinds()")]
        public void SaveBindings() {
#if UNITY_EDITOR
            SetDataBindsInEditor();
#endif

            SaveAllDataBinds();
        }

        [ContextMenu("LoadAllDataBinds()")]
        public void LoadDataBinds() {
#if UNITY_EDITOR
            SetDataBindsInEditor();
#endif

            LoadAllDataBinds();
        }

        [ContextMenu("WriteToNewGameDataCall()")]
        public void WriteToNewGameDataCall() {
            WriteToNewGameData();
        }

        [ContextMenu("LoadFromNewGameDataCall()")]
        public void LoadFromNewGameDataCall() {
            gameData = (SaveGameData)newGameData.gameData.Clone();
        }

        [ContextMenu("ResetGameData()")]
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
#endif
    }
}