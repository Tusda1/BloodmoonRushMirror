using System;

namespace Utilities.DataBinding {
    public interface IGameData : ICloneable {
        public string Name { get; set; }
    }

    public interface ISaveable : ICloneable {
        string Id { get; set; }
    }
}