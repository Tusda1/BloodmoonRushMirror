using System;
using Attributes;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI.Settings {
    public class ControlsSettingsMenu : MonoBehaviour, IMenu {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector]
        private UIDocument uiDocument;
        
        private VisualElement _root;
        private VisualElement _controlsSettingsMenu;
        private RadioButtonGroup _radioButtonGroup;

        public static string ControlScheme = "Keyboard";

        public event Action OnMenuActivated = delegate { };
        public event Action OnMenuDeactivated = delegate { };
        
        [Inject] private IControlSchemeChanger _controlSchemeChanger;
        #endregion

        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _controlsSettingsMenu = _root.Q<VisualElement>("ControlsMenu");
            
            ControlScheme = PlayerPrefs.GetString("ControlScheme", "KeyboardMouse");
            
            _radioButtonGroup = _controlsSettingsMenu.Q<RadioButtonGroup>();

            _radioButtonGroup.RegisterValueChangedCallback(evt => {
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

        [ContextMenu(nameof(SetControllerScheme))]
        public void SetControllerScheme() {
            ControlScheme = "Gamepad";
            
            SetControlScheme(in ControlScheme);
        }

        [ContextMenu(nameof(SetKeyboardMouseScheme))]
        public void SetKeyboardMouseScheme() {
            ControlScheme = "KeyboardMouse";
            
            SetControlScheme(in ControlScheme);
        }

        [ContextMenu(nameof(SetKeyboardScheme))]
        public void SetKeyboardScheme() {
            ControlScheme = "Keyboard";
            
            SetControlScheme(in ControlScheme);
        }

        private void SetControlScheme(in string newScheme) {
            _controlSchemeChanger.ChangeSchemeByName(newScheme);
            PlayerPrefs.SetString("ControlScheme", newScheme);
        }

        public void Activate() {
            _controlsSettingsMenu.SetEnabled(true);
            _controlsSettingsMenu.RemoveFromClassList("left");
            _controlsSettingsMenu.RemoveFromClassList("right");
            _radioButtonGroup.Focus();
            
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