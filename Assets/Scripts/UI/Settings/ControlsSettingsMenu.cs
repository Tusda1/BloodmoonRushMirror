using System;
using Attributes;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Settings {
    public class ControlsSettingsMenu : MonoBehaviour, IMenu {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector]
        private UIDocument uiDocument;
        
        private VisualElement _root;
        private VisualElement _controlsSettingsMenu;

        private Button _controllerSchemeButton;
        private Button _keyboardMouseSchemeButton;
        private Button _keyboardSchemeButton;

        public event Action OnMenuActivated = delegate { };
        public event Action OnMenuDeactivated = delegate { };
        #endregion

        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _controlsSettingsMenu = _root.Q<VisualElement>("ControlsMenu");
            
            _controllerSchemeButton = _root.Q<Button>("ControllerSchemeButton");
            _keyboardMouseSchemeButton = _root.Q<Button>("KeyboardMouseSchemeButton");
            _keyboardSchemeButton = _root.Q<Button>("KeyboardSchemeButton");

            _controllerSchemeButton.clicked += SetControllerScheme;
            _keyboardMouseSchemeButton.clicked += SetKeyboardMouseScheme;
            _keyboardSchemeButton.clicked += SetKeyboardScheme;
        }

        private void SetControllerScheme() {
            throw new NotImplementedException();
        }

        private void SetKeyboardMouseScheme() {
            throw new NotImplementedException();
        }

        private void SetKeyboardScheme() {
            throw new NotImplementedException();
        }

        public void Activate() {
            _controlsSettingsMenu.SetEnabled(true);
            _controlsSettingsMenu.RemoveFromClassList("left");
            _controlsSettingsMenu.RemoveFromClassList("right");
            _controllerSchemeButton.Focus();
            
            OnMenuActivated.Invoke();
        }

        public void Deactivate(string addToClassList) {
            _controlsSettingsMenu.SetEnabled(false);

            if (addToClassList != null) {
                _controlsSettingsMenu.AddToClassList(addToClassList);
            }
            
            OnMenuDeactivated.Invoke();
        }
    }
}