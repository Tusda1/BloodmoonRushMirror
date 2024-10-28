using System;

namespace Utilities.DataBinding {
    public interface IGameData : ICloneable {
        public string Name { get; set; }
    }

    /// <summary>
    /// 'id' must be serialized with '[field: SerializeField]' to be visible in the inspector
    /// </summary>
    public interface ISaveable {
        string ID { get; set; }
    }
}