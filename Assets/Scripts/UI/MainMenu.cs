using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace UI {
    public class MainMenu : MonoBehaviour {
        [SerializeField] private UIDocument uiDocument;

        [SerializeField] private InputActionReference tabNavigateAction;

        private VisualElement _rootElement;

        private Button _startButton;
        private Button _settingsButton;
        private Button _quitButton;

        private Button _menuButton;

        private TabView _tabbedView;

        private VisualElement _mainMenu;
        private VisualElement _settings;

        private List<Tab> _tabs;
        private int _tabIndex;

        private void Awake() {
            FmodManager.Instance.SetParameterValue("levelMusicProgression", 0);
            FmodManager.Instance.PlayMusic("event:/Music/Music_level1");
            
            _rootElement = uiDocument.rootVisualElement;

            _startButton = _rootElement.Q<Button>("StartButton");
            _settingsButton = _rootElement.Q<Button>("SettingsButton");
            _quitButton = _rootElement.Q<Button>("QuitButton");
        
            _menuButton = _rootElement.Q<Button>("MenuButton");

            _mainMenu = _rootElement.Q<VisualElement>("MainMenu");
            _settings = _rootElement.Q<VisualElement>("Settings");

            _tabbedView = _rootElement.Q<TabView>("TabbedView");
            _tabs = _rootElement.Query(className: "tab").OfType<Tab>().ToList();

            _startButton.clicked += StartButtonClicked;
            _settingsButton.clicked += SettingsButtonClicked;
            _quitButton.clicked += QuitButtonClicked;

            _menuButton.clicked += SettingsButtonClicked;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            _settings.enabledSelf = false;
        }

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

            _tabIndex = math.clamp(_tabIndex, 0, _tabs.Count - 1);

            _tabbedView.activeTab = _tabs[_tabIndex];
        }

        #endregion


        private void StartButtonClicked() {
            SceneManager.LoadScene("CutScene");
        }

        private void SettingsButtonClicked() {
            if (_mainMenu.ClassListContains("left")) {
                _mainMenu.RemoveFromClassList("left");
                _settings.AddToClassList("right");

                _mainMenu.enabledSelf = true;
                _settings.enabledSelf = false;
            }
            else {
                _mainMenu.AddToClassList("left");
                _settings.RemoveFromClassList("right");

                _mainMenu.enabledSelf = false;
                _settings.enabledSelf = true;
            }
        }

        private void QuitButtonClicked() {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
    }
}