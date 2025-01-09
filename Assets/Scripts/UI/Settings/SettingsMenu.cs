using Attributes;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Utilities;

namespace UI.Settings {
    [RequireComponent(typeof(VideoSettingsMenu), typeof(AudioSettingsMenu), typeof(ControlsSettingsMenu))]
    [RequireComponent(typeof(UITK_WorldSpaceInteractive))]
    public class SettingsMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;

        [SerializeField] private InputActionReference tabNavigateAction;
        [SerializeField] private InputActionReference backAction;

        #region ---PrivateFields---
        private VisualElement _root;
        private VisualElement _settingsMenuRoot;

        private IndexedArray<VisualElement> _menus;
        
        private BatArrow _leftBat;
        private BatArrow _rightBat;
        #endregion
        #endregion

        #region ---EventFunctions---
        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _settingsMenuRoot = _root.Q<VisualElement>("SettingsMenuRoot");
            _leftBat = _settingsMenuRoot.Q<BatArrow>("LeftBat");
            _rightBat = _settingsMenuRoot.Q<BatArrow>("RightBat");

            var menus = _settingsMenuRoot.Query(className: "menu").OfType<VisualElement>().ToList();
            _menus = new IndexedArray<VisualElement>(menus.ToArray());
            _menus.OnIndexChanged += SetActiveMenu;
            _menus.OnMinIndex += _leftBat.Disable;
            _menus.NotOnMinIndex += () => _leftBat.Enable(() => {
                Debug.Log("Left Bat pressed");
                _menus.DecreaseIndex();
            });
            _menus.OnMaxIndex += _rightBat.Disable;
            _menus.NotOnMaxIndex += () => _rightBat.Enable(() => {
                Debug.Log("Right Bat pressed");
                _menus.IncreaseIndex();
            });
            _menus.SetIndex(0);
            
            _settingsMenuRoot.AddToClassList("down");
            _settingsMenuRoot.SetEnabled(false);
        }
        #endregion

        public void ActivateSettingsMenu() {
            _settingsMenuRoot.SetEnabled(true);
            _settingsMenuRoot.RemoveFromClassList("down");
            tabNavigateAction.action.performed += TabNavigation;
            backAction.action.performed += DeactivateSettingsMenu;
            
            Debug.Log("SettingsMenu Activated");
        }

        public void DeactivateSettingsMenu() {
            _settingsMenuRoot.AddToClassList("down");
            tabNavigateAction.action.performed -= TabNavigation;
            backAction.action.performed -= DeactivateSettingsMenu;
            _settingsMenuRoot.SetEnabled(false);
        }

        private void DeactivateSettingsMenu(InputAction.CallbackContext obj) {
            DeactivateSettingsMenu();
        }

        private void TabNavigation(InputAction.CallbackContext obj) {
            var value = (int)obj.ReadValue<float>();
            Debug.Log("TabNavigation: " + value);
            
            switch (value) {
                case -1:
                    _leftBat.TriggerOnClick();
                    break;
                case 1:
                    _rightBat.TriggerOnClick();
                    break;
            }
        }

        private void SetActiveMenu(VisualElement currentMenu) {
            var pos = _menus.PreviousIndex < _menus.CurrentIndex;
            _menus.PreviousItem.AddToClassList(pos ? "left" : "right");
            _menus.PreviousItem.SetEnabled(false);

            Debug.Log(currentMenu.name);
            
            currentMenu.RemoveFromClassList("left");
            currentMenu.RemoveFromClassList("right");
            currentMenu.SetEnabled(true);
        }
    }
}