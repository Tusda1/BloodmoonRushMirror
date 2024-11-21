using Attributes;
using FMODScripts;
using UI.Settings;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI {
    [RequireComponent(typeof(SettingsMenu))]
    public class MainMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField] private UIDocument _uiDocument;
        
        [SerializeField] private SceneCollectionSO startSceneCollectionSO;

        private VisualElement _rootElement;

        private Button _startButton;
        private Button _settingsButton;
        private Button _quitButton;

        private Button _menuButton;

        private VisualElement _mainMenu;
        private VisualElement _settings;
        
        private static FMODManager FMODManager => FMODManager.Instance;
        #endregion

        private void Awake() {
            _rootElement = _uiDocument.rootVisualElement;

            _startButton = _rootElement.Q<Button>("StartButton");
            _settingsButton = _rootElement.Q<Button>("SettingsButton");
            _quitButton = _rootElement.Q<Button>("QuitButton");
        
            _menuButton = _rootElement.Q<Button>("MenuButton");

            _mainMenu = _rootElement.Q<VisualElement>("MainMenuRoot");
            _settings = _rootElement.Q<VisualElement>("SettingsMenuRoot");


            _startButton.clicked += StartButtonClicked;
            _settingsButton.clicked += SettingsButtonClicked;
            _quitButton.clicked += QuitButtonClicked;

            _menuButton.clicked += SettingsButtonClicked;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            _settings.enabledSelf = false;
        }

        private void Start() {
            FMODManager.SetParameterValue("levelMusicProgression", 0);
            FMODManager.PlayMusic("event:/Music/Music_level1");
        }

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

        private static void QuitButtonClicked() {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
    }
}