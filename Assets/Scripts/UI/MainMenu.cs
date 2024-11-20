using System.Collections.Generic;
using Attributes;
using FMODScripts;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI {
    public class MainMenu : MonoBehaviour {
        [GetOrAddComponent] private UIDocument _uiDocument;

        [SerializeField] private InputActionReference tabNavigateAction;
        
        [SerializeField] private SceneCollectionSO startSceneCollectionSO;

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
        
        private static FMODManager FMODManager => FMODManager.Instance;

        private void Start() {
            FMODManager.SetParameterValue("levelMusicProgression", 0);
            FMODManager.PlayMusic("event:/Music/Music_level1");
            
            _rootElement = _uiDocument.rootVisualElement;

            _startButton = _rootElement.Q<Button>("StartButton");
            _settingsButton = _rootElement.Q<Button>("SettingsButton");
            _quitButton = _rootElement.Q<Button>("QuitButton");
        
            _menuButton = _rootElement.Q<Button>("MenuButton");

            _mainMenu = _rootElement.Q<VisualElement>("MainMenuRoot");
            _settings = _rootElement.Q<VisualElement>("SettingsMenuRoot");

            _tabbedView = _rootElement.Q<TabView>("TabbedView");
            _tabs = _rootElement.Query(className: "tab").OfType<Tab>().ToList();

            _startButton.clicked += StartButtonClicked;
            _settingsButton.clicked += SettingsButtonClicked;
            _quitButton.clicked += QuitButtonClicked;

            // _menuButton.clicked += SettingsButtonClicked;

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
            _ = SceneCollectionManager.LoadSceneCollection(startSceneCollectionSO);
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