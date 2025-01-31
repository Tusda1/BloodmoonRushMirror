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

        public static string ControlScheme = "KeyboardMouse";

        public event Action OnMenuActivated = delegate { };
        public event Action OnMenuDeactivated = delegate { };
        #endregion

        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _controlsSettingsMenu = _root.Q<VisualElement>("ControlsMenu");
            
            ControlScheme = PlayerPrefs.GetString("ControlScheme", "KeyboardMouse");
            
            var p = _controlsSettingsMenu.Q<RadioButtonGroup>();

            p.RegisterValueChangedCallback(evt => {
                switch (evt.newValue) {
                    case 1:
                        SetControllerScheme();
                        break;
                    case 2:
                        SetKeyboardMouseScheme();
                        break;
                    case 3:
                        SetKeyboardScheme();
                        break;
                }
            });

        }

        private void SetControllerScheme() {
            ControlScheme = "Controller";
            
            PlayerPrefs.SetString("ControlScheme", ControlScheme);
        }

        private void SetKeyboardMouseScheme() {
            ControlScheme = "KeyboardMouse";
            
            PlayerPrefs.SetString("ControlScheme", ControlScheme);
        }

        private void SetKeyboardScheme() {
            ControlScheme = "Keyboard";
            
            PlayerPrefs.SetString("ControlScheme", ControlScheme);
        }

        public void Activate() {
            _controlsSettingsMenu.SetEnabled(true);
            _controlsSettingsMenu.RemoveFromClassList("left");
            _controlsSettingsMenu.RemoveFromClassList("right");
            // _controllerSchemeButton.Focus();
            
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