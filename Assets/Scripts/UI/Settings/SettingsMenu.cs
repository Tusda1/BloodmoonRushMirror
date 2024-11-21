using System.Collections.Generic;
using Attributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UI.Settings {
    [RequireComponent(typeof(VideoSettingsMenu), typeof(AudioSettingsMenu), typeof(ControlsSettingsMenu))]
    public class SettingsMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        [SerializeField] private InputActionReference tabNavigateAction;
        
        private VisualElement _settings;
        private List<Tab> _tabs;
        private TabView _tabView;
        private int _tabIndex;
        #endregion

        #region ---EventFunctions---
        private void Start() {
        }

        private void OnEnable() {
            tabNavigateAction.action.performed += ChangeTabIndex;
        }
        
        private void OnDisable() {
            tabNavigateAction.action.performed -= ChangeTabIndex;
        }
        #endregion
        
        private void ChangeTabIndex(InputAction.CallbackContext obj) {
            if (!_settings.enabledSelf) return;
        
            _tabIndex += (int)obj.ReadValue<float>();
        
            _tabIndex = math.clamp(_tabIndex, 0, _tabs.Count - 1);
        
            _tabView.activeTab = _tabs[_tabIndex];
        }
    }
}