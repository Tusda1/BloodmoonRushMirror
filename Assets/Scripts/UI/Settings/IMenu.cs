using System;
using JetBrains.Annotations;

namespace UI.Settings {
    public interface IMenu {
        event Action OnMenuActivated;
        event Action OnMenuDeactivated;
        
        void Activate();
        void Deactivate([CanBeNull] string addToClassList = null);
    }
}