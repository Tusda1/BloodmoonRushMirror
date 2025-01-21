using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serializers;
using Zenject;
using Application = UnityEngine.Device.Application;

namespace Utilities.DataBinding {
    /// <summary>
    /// If serializer is not specified FileDataService will use a JsonSerializer
    /// </summary>
    public class FileDataService<TGameData> : IDataService<TGameData> where TGameData : IGameData {
        public ISerializer Serializer { get; }
        public string DataPath { get; }
        public string FileExtension { get; }
        
        [Inject]
        public FileDataService(ISerializer serializer) {
            Serializer = serializer;
            
            DataPath = Path.Combine(Application.persistentDataPath, typeof(TGameData).Name);
            if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);

            FileExtension = Serializer.FileExtension;
        }
        
        public void OpenFileLocation() {
            System.Diagnostics.Process.Start(DataPath);
        }

        public string GetPathToSaveFile(string fileName) {
            return Path.Combine(DataPath, fileName)  + FileExtension;
        }

        public void Save(TGameData gameData) {
            var filePath = GetPathToSaveFile(gameData.Name);

            File.WriteAllText(filePath, Serializer.Serialize(gameData));
        }

        #region ---Loading---
        public TGameData LoadByName(string name) {
            var filePath = GetPathToSaveFile(name);

            return LoadByPath(filePath);
        }

        public TGameData LoadByPath(string path) {
            if (!File.Exists(path)) {
                throw new ArgumentException($"No save file with the path: '{path}'.");
            }

            return Serializer.Deserialize<TGameData>(File.ReadAllText(path));
        }
        #endregion

        #region ---Deletion---
        public void DeleteByName(string name) {
            var filePath = GetPathToSaveFile(name);

            DeleteByPath(filePath);
        }

        public void DeleteByPath(string path) {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }

        public void DeleteAll() {
            foreach (var saveFile in ListSavesPath()) {
                File.Delete(saveFile);
            }
        }
        #endregion

        #region ---ReadFiles---
        public IEnumerable<string> ListSavesPath() {
            return Directory.GetFiles(DataPath).Where(file => file.EndsWith(FileExtension));
        }

        public FileInfo ReadFileMetadata(string path) {
            return new FileInfo(path);
        }

        public FileInfo[] ListFiles() {
            return ListSavesPath().Select(ReadFileMetadata).ToArray();
        }

        public IEnumerable<string> ListSavesName() {
            return ListFiles().Select(file => file.Name);
        }

        public FileInfo GetMostRecentSaveFile() {
            return ListFiles().OrderBy(x => x.LastWriteTime).LastOrDefault();
        }

        public int GetSaveCount() {
            return ListSavesPath().Count();
        }
        #endregion
    }
}