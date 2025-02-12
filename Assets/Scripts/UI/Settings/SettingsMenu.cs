using System;
using Attributes;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Utilities;
using VContainer;

namespace UI.Settings {
    [RequireComponent(typeof(VideoSettingsMenu), typeof(AudioSettingsMenu), typeof(ControlsSettingsMenu))]
    [RequireComponent(typeof(UITKWorldSpaceInteractive))]
    public class SettingsMenu : MonoBehaviour, IMenu {
        #region ---Fields---
        #region ***---GetOrAddComponents---***
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        [GetOrAddComponent, SerializeField, HideInInspector] private AudioSettingsMenu audioSettingsMenu; 
        [GetOrAddComponent, SerializeField, HideInInspector] private VideoSettingsMenu videoSettingsMenu;
        [GetOrAddComponent, SerializeField, HideInInspector] private ControlsSettingsMenu controlsSettingsMenu;
        #endregion
        
        [SerializeField] private InputActionReference tabNavigateAction;
        [SerializeField] private InputActionReference backAction;

        public event Action OnMenuActivated = delegate { };
        public event Action OnMenuDeactivated = delegate { };

        #region ---PrivateFields---
        private VisualElement _root;
        private VisualElement _settingsMenuRoot;

        private IndexedArray<IMenu> _menus;

        private BatArrow _leftBat;
        private BatArrow _rightBat;
        #endregion
        
        [Inject] private IAudioManager _audioManager;
        
        private const string GenericMenuSound_SoundID = "UI_GenericMenuSound";
        #endregion

        #region ---Initialization---
        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _settingsMenuRoot = _root.Q<VisualElement>("SettingsMenuRoot");
            _leftBat = _settingsMenuRoot.Q<BatArrow>("LeftBat");
            _rightBat = _settingsMenuRoot.Q<BatArrow>("RightBat");
        }

        private void Start() {
            var menus = new IMenu[] { audioSettingsMenu, videoSettingsMenu, controlsSettingsMenu };
            _menus = new IndexedArray<IMenu>(menus);

            _leftBat.SetOnClickAction(_menus.DecreaseIndex);
            _rightBat.SetOnClickAction(_menus.IncreaseIndex);

            _menus.OnIndexChanged += SetActiveMenu;
            _menus.OnMinIndex += _leftBat.Disable;
            _menus.NotOnMinIndex += () => _leftBat.Enable();
            _menus.OnMaxIndex += _rightBat.Disable;
            _menus.NotOnMaxIndex += () => _rightBat.Enable();

            _settingsMenuRoot.AddToClassList("down");
            _settingsMenuRoot.SetEnabled(false);
        }
        #endregion

        public void Activate() {
            _settingsMenuRoot.SetEnabled(true);
            _settingsMenuRoot.RemoveFromClassList("down");
            tabNavigateAction.action.performed += TabNavigation;
            backAction.action.performed += Deactivate;
            _menus.SetIndex(_menus.CurrentIndex);

            Debug.Log("SettingsMenu Activated");

            OnMenuActivated.Invoke();
        }

        public void Deactivate(string addToClassList = null) {
            _settingsMenuRoot.AddToClassList("down");
            tabNavigateAction.action.performed -= TabNavigation;
            backAction.action.performed -= Deactivate;
            _settingsMenuRoot.SetEnabled(false);

            Debug.Log("SettingsMenu Deactivated");

            OnMenuDeactivated.Invoke();
        }

        private void Deactivate(InputAction.CallbackContext obj) {
            Deactivate();
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

        private void SetActiveMenu(IMenu currentMenu, IMenu previousMenu, bool indexIncreased) {
            previousMenu.Deactivate(indexIncreased ? "left" : "right");
            currentMenu.Activate();
            
            _audioManager.PlayOneShot(GenericMenuSound_SoundID);
        }
    }
}