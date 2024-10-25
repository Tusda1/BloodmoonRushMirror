using System;

namespace Utilities.DataBinding {
    public interface IGameData : ICloneable {
        public string name { get; set; }
    }

    public interface ISaveable {
        string id { get; set; }
    }
}