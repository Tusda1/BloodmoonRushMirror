using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;
using Zenject;
using Debug = UnityEngine.Debug;

namespace SaveLoadSystem {
    public class SaveLoadSystemManager : SaveLoadSystemBase<SaveLoadSystemManager, SaveGameData> {
        #region ---Fields---
        #region ***---DI---***
        [Inject(Id = "Slot1")] private SaveSlotDataSO _saveSlot1Data;
        [Inject(Id = "Slot2")] private SaveSlotDataSO _saveSlot2Data;
        [Inject(Id = "Slot3")] private SaveSlotDataSO _saveSlot3Data;
        #endregion
        #endregion
        
        [Inject]
        public void Init(FileDataService<SaveGameData> dataService) {
            DataService = dataService;
        }
        
        #region ---SaveSlots---
        public void UpdateSaveSlotSO() {
            LoadSaveSlotData(_saveSlot1Data);
            LoadSaveSlotData(_saveSlot2Data);
            LoadSaveSlotData(_saveSlot3Data);
        }

        private void LoadSaveSlotData(SaveSlotDataSO saveSlotData) {
            saveSlotData.Clear();
            
            var pathToSaveFile = DataService.GetPathToSaveFile(saveSlotData.saveSlotName);
            if (!File.Exists(pathToSaveFile)) {
                Debug.Log(pathToSaveFile + " does not exist!");
                return;
            }
            
            var metaData = DataService.ReadFileMetadata(pathToSaveFile);
            saveSlotData.lastTimePlayed = metaData.LastWriteTime.ToString("HH:mm dd/MM/yyyy");
        }
        #endregion

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
                instance = this;
                
                SetDataBindsInEditor();
            }
#endif

            SaveAllDataBinds();
        }

        [ContextMenu(nameof(LoadDataBinds))]
        public void LoadDataBinds() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                instance = this;
                
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

                DebugLogger.Log($"{dataBinds.Length} data binds saved successfully.");
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

        [ContextMenu(nameof(OpenPersistantDataPath))]
        private void OpenPersistantDataPath() {
            Process.Start(Application.persistentDataPath);
        }
#endif
    }
}