using System;
using Attributes;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI.Settings {
    public class ControlsSettingsMenu : MonoBehaviour, IMenu {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        private VisualElement _root;
        private VisualElement _controlsSettingsMenu;
        private SpriteElement _controlsLayout;
        private DropdownField _controlsLayoutDropdown;

        public static string ControlScheme;

        public event Action OnMenuActivated = delegate { };
        public event Action OnMenuDeactivated = delegate { };
        
        [Inject] private IControlSchemeChanger _controlSchemeChanger;
        #endregion

        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _controlsSettingsMenu = _root.Q<VisualElement>("ControlsMenu");
            _controlsLayout = _controlsSettingsMenu.Q<SpriteElement>("ControlsLayout");
            _controlsLayoutDropdown = _controlsSettingsMenu.Q<DropdownField>("LayoutDropdown");
        }

        private void Start() {
            _controlsLayoutDropdown.RegisterValueChangedCallback(evt => {
                switch (evt.newValue) {
                    case "Keyboard + Mouse":
                        SetKeyboardMouseScheme();
                        break;
                    case "Keyboard":
                        SetKeyboardScheme();
                        break;
                    case "Controller":
                        SetControllerScheme();
                        break;
                }
            });
            
            ControlScheme = _controlSchemeChanger.CurrentControlSchemeName;
            
            _controlSchemeChanger.OnControlSchemeChanged += SetControlsLayoutImage;
            SetControlsLayoutImage(_controlSchemeChanger.CurrentControlSchemeData);
        }

        private void SetControlsLayoutImage(IControlSchemeChanger.ControlSchemeData controlSchemeData) {
            _controlsLayout.Sprite = controlSchemeData.DataSO.controlsLayout;
        }

        #region ---SetScheme---
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
        #endregion

        private void SetControlScheme(in string newScheme) {
            _controlSchemeChanger.ChangeSchemeByName(newScheme);
        }

        public void Activate() {
            _controlsSettingsMenu.SetEnabled(true);
            _controlsSettingsMenu.RemoveFromClassList("left");
            _controlsSettingsMenu.RemoveFromClassList("right");
            _controlsLayoutDropdown.Focus();
            
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