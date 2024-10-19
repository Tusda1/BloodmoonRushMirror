using System.Collections.Generic;

namespace Utilities.DataBinding {
    public interface IDataService<TGameData> where TGameData : IGameData {
        void Save(TGameData gameData);
        TGameData Load(string name);
        void Delete(string name);
        void DeleteAll();
        IEnumerable<string> ListSavesPath();
        IEnumerable<string> ListSavesName();
        int GetSaveCount();
    }
}