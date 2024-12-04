using Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Settings {
    public class ControlsSettingsMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector]
        private UIDocument uiDocument;
        
        private VisualElement _root;
        #endregion

        private void Awake() {
            _root = uiDocument.rootVisualElement;
        }
    }
}