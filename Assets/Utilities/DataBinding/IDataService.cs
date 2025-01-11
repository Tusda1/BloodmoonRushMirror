using System.Collections.Generic;
using System.IO;
using Serializers;

namespace Utilities.DataBinding {
    public interface IDataService<TGameData> where TGameData : IGameData {
        public ISerializer Serializer { get;}
        public string DataPath { get;}
        public string FileExtension { get;}
        
        string GetPathToSaveFile(string filename);
        void Save(TGameData gameData);
        TGameData LoadByName(string name);
        TGameData LoadByPath(string path);
        void DeleteByName(string name);
        void DeleteByPath(string path);
        void DeleteAll();
        FileInfo[] ListFiles();
        FileInfo ReadFileMetadata(string path);
        IEnumerable<string> ListSavesPath();
        IEnumerable<string> ListSavesName();
        FileInfo GetMostRecentSaveFile();
        int GetSaveCount();
    }
}