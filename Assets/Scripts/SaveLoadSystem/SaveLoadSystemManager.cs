using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Attributes;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;

namespace SaveLoadSystem {
    public class SaveLoadSystemManager : SaveLoadSystemBase<SaveLoadSystemManager, SaveGameData> {
        [SerializeField, Required] private SaveSlotData saveSlot1Data;
        [SerializeField, Required] private SaveSlotData saveSlot2Data;
        [SerializeField, Required] private SaveSlotData saveSlot3Data;
        
        protected override void Awake() {
            base.Awake();

            DataService = new FileDataService<SaveGameData>();

            LoadAllSaveSlotDatas();
        }

        #region ---SaveSlots---
        [ContextMenu("SaveFiles/" + nameof(LoadAllSaveSlotDatas))]
        private void LoadAllSaveSlotDatas() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                DataService ??= new FileDataService<SaveGameData>();
            }
#endif
            LoadSaveSlotData(saveSlot1Data);
            LoadSaveSlotData(saveSlot2Data);
            LoadSaveSlotData(saveSlot3Data);
        }

        private void LoadSaveSlotData(SaveSlotData saveSlotData) {
            var pathToSaveFile = DataService.GetPathToSaveFile(saveSlotData.saveSlotName);
            if (!File.Exists(pathToSaveFile)) {
                Debug.Log(pathToSaveFile + " does not exist!");
                return;
            }
            
            var metaData = DataService.ReadFileMetadata(pathToSaveFile);
            saveSlotData.lastTimePlayed = metaData.LastWriteTime.ToString("HH:mm dd/MM/yyyy");
        }
        
#if UNITY_EDITOR
        [ContextMenu("SaveFiles/" + nameof(OpenFileLocation))]
        private void OpenFileLocation() {
            DataService ??= new FileDataService<SaveGameData>();
            System.Diagnostics.Process.Start(DataService.DataPath);
        }
#endif
        #endregion

        [ContextMenu("Runtime/" + nameof(SaveToFile))]
        public void SaveToFile() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                DataService ??= new FileDataService<SaveGameData>();
            }
#endif
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
#endif
    }
}