using System.Collections.Generic;
using Attributes;
using Player;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Settings {
    [RequireComponent(typeof(VideoSettingsMenu), typeof(AudioSettingsMenu), typeof(ControlsSettingsMenu))]
    public class SettingsMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;

        #region ---PrivateFields---
        private PlayerScript _player;

        private VisualElement _root;
        private VisualElement _settings;
        private readonly List<Tab> _tabs = new();
        private TabView _tabView;
        private int _tabIndex;
        #endregion
        #endregion

        #region ---EventFunctions---
        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _settings = _root.Q<VisualElement>("Settings");
            _settings.Query(className: "tab")
                .Where(x => x is Tab)
                .ForEach(x => _tabs.Add(x as Tab));
        }

        private void Start() {
            _player = ServiceLocator.Get<PlayerScript>();
            _player.TabNavigation += ChangeTabIndex;
        }
        #endregion

        #region ---PrivateFunctions---
        private void ChangeTabIndex(int change) {
            if (!_settings.enabledSelf) return;

            _tabIndex += change;

            _tabIndex = math.clamp(_tabIndex, 0, _tabs.Count - 1);

            _tabView.activeTab = _tabs[_tabIndex];
        }
        #endregion
    }
}