using System;

namespace Utilities.DataBinding {
    public interface IGameData : ICloneable {
        public string name { get; set; }
    }

    /// <summary>
    /// 'id' must be serialized with '[field: SerializeField]' to be visible in the inspector
    /// </summary>
    public interface ISaveable {
        string id { get; set; }
    }
}