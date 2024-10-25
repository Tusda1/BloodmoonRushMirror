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
    
    public interface IBind<TData> : IDataBind {
        public string id { get; set; }
        TData saveSystemData { get; set; }
        TData dataInGameData { get; }
    }
}