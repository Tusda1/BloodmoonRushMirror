namespace Utilities.DataBinding {
    public interface IDataBind {
        /// <summary>
        /// "LoadBindData" is done by the class implementing "IBind".
        /// </summary>
        void LoadBindData();

        void LoadCheckpointBindData() {
        }

        /// <summary>
        /// "SaveBindData" is called when a new save is made.
        /// Implementation needs to add themselves to the "GameDataBinds" list in the SaveLoadSystem to be updated.
        /// </summary>
        void SaveBindData();

        /// <summary>
        /// The example should be the implementation most of the time
        /// </summary>
        /// <code>saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
        void Bind();
    }

    public interface IBind<TData> : IDataBind {
        public string ID { get; set; }
        TData SaveSystemData { get; set; }
        TData DataInGameData { get; }
    }
}