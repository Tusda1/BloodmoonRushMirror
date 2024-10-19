using System.Collections.Generic;
using System.IO;

namespace Utilities.DataBinding {
    public interface IDataService<TGameData> where TGameData : IGameData {
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