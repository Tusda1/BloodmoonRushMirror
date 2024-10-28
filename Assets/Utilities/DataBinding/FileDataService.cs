using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Encryption;
using Serializers;
using Application = UnityEngine.Device.Application;

namespace Utilities.DataBinding {
    /// <summary>
    /// If serializer is not specified FileDataService will use a JsonSerializer
    /// </summary>
    public class FileDataService<TGameData> : IDataService<TGameData> where TGameData : IGameData {
        private readonly ISerializer _serializer;
        private readonly string _dataPath;
        private readonly string _fileExtension;

        public FileDataService(ISerializer serializer = null, bool encrypted = false) {
            _serializer = encrypted ? new EncryptedSerializer(serializer) : serializer ?? new JSONSerializer();

            _dataPath = Path.Combine(Application.persistentDataPath, typeof(TGameData).Name);
            if (!Directory.Exists(_dataPath)) Directory.CreateDirectory(_dataPath);

            _fileExtension = _serializer.FileExtension;
        }

        private string GetPathToSaveFile(string fileName) {
            return Path.Combine(_dataPath, fileName);
        }

        public void Save(TGameData gameData) {
            var filePath = GetPathToSaveFile(gameData.name) + _fileExtension;

            File.WriteAllText(filePath, _serializer.Serialize(gameData));
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

            return _serializer.Deserialize<TGameData>(File.ReadAllText(path));
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
            return Directory.GetFiles(_dataPath).Where(file => file.EndsWith(_fileExtension));
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