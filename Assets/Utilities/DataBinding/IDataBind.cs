namespace Utilities.DataBinding {
    public interface IDataBind {
        /// <summary>
        /// "LoadBindData" is done by the class implementing "IBind".
        /// </summary>
        void LoadBindData();
        
        /// <summary>
        /// "SaveBindData" is called when a new save is made.
        /// Implementation needs to add themselves to the "GameDataBinds" list in the SaveLoadSystem to be updated.
        /// </summary>
        void SaveBindData();
    }
    
    public interface IBind<TData> : IDataBind where TData : ISaveable {
        string Id { get; }
        TData GameData { get; set; }
        TData DataInGameData { get; }
        
        /// <summary>
        /// "Bind" is called by the class implementing the interface; usually when created.
        /// </summary>
        void Bind(TData data);
        
        /// <summary>
        /// "UnBind" is called by the class implementing the interface; usually when destroyed.
        /// </summary>
        void UnBind(TData data);
    }
}