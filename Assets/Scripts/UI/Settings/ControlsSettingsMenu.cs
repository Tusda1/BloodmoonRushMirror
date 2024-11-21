using Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Settings {
    public class ControlsSettingsMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        private Tab _controlsTab;
        #endregion

        private void Awake() {
            _controlsTab = uiDocument.rootVisualElement.Q<Tab>("ControlsTab");
        }
    } 
}
