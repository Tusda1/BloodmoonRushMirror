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
        
        public FileDataService(ISerializer serializer = null, bool encrypted = true) {
            _serializer = encrypted ? new EncryptedSerializer(serializer) : serializer ?? new JSONSerializer();
            
            _dataPath = Application.persistentDataPath + "/" + typeof(TGameData).Name;
            if (!Directory.Exists(_dataPath)) Directory.CreateDirectory(_dataPath);
            
            _fileExtension = _serializer.FileExtension;
        }

        private string GetPathToSaveFile(string fileName) => Path.Combine(_dataPath, string.Concat(fileName, _fileExtension));

        public void Save(TGameData gameData) {
            gameData.LastSaveTime = DateTime.Now;
            var filePath = GetPathToSaveFile(gameData.Name);
            
            File.WriteAllText(filePath, _serializer.Serialize(gameData));
        }

        public TGameData Load(string name) {
            var filePath = GetPathToSaveFile(name);
            
            if (!File.Exists(filePath)) {
                throw new ArgumentException($"No save file with the name: '{name}'.");
            }
            
            return _serializer.Deserialize<TGameData>(File.ReadAllText(filePath));
        }

        public void Delete(string name) {
            var filePath = GetPathToSaveFile(name);

            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
        }

        public void DeleteAll() {
            foreach (var saveFile in ListSavesPath()) {
                File.Delete(saveFile);
            }
        }
        
        public IEnumerable<string> ListSavesPath() {
            return Directory.GetFiles(_dataPath).Where(file => file.EndsWith(_fileExtension));
        }
        
        public IEnumerable<string> ListSavesName() {
            return Directory.GetFiles(_dataPath).Where(file => file.EndsWith(_fileExtension))
                .Select(file => file.Substring(_dataPath.Length + 1, file.Length - _dataPath.Length - _fileExtension.Length - 1));
        }

        public int GetSaveCount() {
            return Directory.GetFiles(_dataPath).Length;
        }
    }
}