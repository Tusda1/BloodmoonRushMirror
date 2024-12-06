using System.Collections.Generic;
using Attributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UI.Settings {
    [RequireComponent(typeof(VideoSettingsMenu), typeof(AudioSettingsMenu), typeof(ControlsSettingsMenu))]
    [RequireComponent(typeof(UITK_WorldSpaceInteractive))]
    public class SettingsMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector]
        private UIDocument uiDocument;

        [SerializeField] private InputActionReference tabNavigateAction;

        #region ---PrivateFields---
        private VisualElement _root;
        private VisualElement _settings;
        private List<Tab> _tabs;
        private TabView _tabView;
        private int _tabIndex;
        #endregion
        #endregion

        #region ---EventFunctions---
        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _settings = _root.Q<VisualElement>("SettingsMenuRoot");
#if UNITY_EDITOR
            Assert.IsNotNull(_settings);
#endif
            _tabView = _root.Q<TabView>("TabbedView");
            _tabs = _root.Query(className: "tab").OfType<Tab>().ToList();
        }
        #endregion

        #region ---InputActions---
        private void OnEnable() {
            tabNavigateAction.action.performed += ChangeTabIndex;
        }

        private void OnDisable() {
            tabNavigateAction.action.performed -= ChangeTabIndex;
        }

        private void ChangeTabIndex(InputAction.CallbackContext obj) {
            if (!_settings.enabledSelf) return;

            _tabIndex += (int)obj.ReadValue<float>();

            _tabIndex = math.clamp(_tabIndex, 0, _tabs.Count);

            _tabView.activeTab = _tabs[_tabIndex];
        }
        #endregion
    }
}