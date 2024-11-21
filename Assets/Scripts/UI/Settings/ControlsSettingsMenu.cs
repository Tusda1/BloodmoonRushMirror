using Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Settings {
    public class ControlsSettingsMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField] private UIDocument _uiDocument;
        
        private Tab _controlsTab;
        #endregion

        private void Awake() {
            _controlsTab = _uiDocument.rootVisualElement.Q<Tab>("ControlsTab");
        }
    } 
}
