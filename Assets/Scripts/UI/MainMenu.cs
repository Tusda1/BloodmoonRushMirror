using System;
using Attributes;
using FMODScripts;
using SceneCollections;
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
        [GetOrAddComponent, SerializeField, HideInInspector]
        private UIDocument uiDocument;

        [SerializeField, Required] private SceneCollectionSO startSceneCollectionSO;
        [SerializeField, Required] private Animator backgroundAnimator;

        private VisualElement _rootElement;
        private VisualElement _mainMenuElement;

        private Label _pressAnyKeyLabel;

        private Button _startButton;
        private Button _settingsButton;
        private Button _quitButton;

        private Button _menuButton;

        private VisualElement _mainMenu;
        private VisualElement _settings;

        private Action _updateActions = delegate { };
        private CameraEffects _cameraEffects;
        #endregion

        private void Awake() {
            _rootElement = uiDocument.rootVisualElement;

            _mainMenuElement = _rootElement.Q<VisualElement>("MainMenu");
            _pressAnyKeyLabel = _rootElement.Q<Label>("PressAnyKeyLabel");

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

            _updateActions += () => {
                if (Input.anyKeyDown) _ = AnyKeyPressed();
            };
        }

        private async Awaitable AnyKeyPressed() {
            _updateActions = delegate { };
            
            backgroundAnimator.Play("MenuZoom");

            for (var i = 0; i < 10; i++) {
                _pressAnyKeyLabel.style.color = new StyleColor(new Color(1, 1, 1, 0.5f));
                await Awaitable.WaitForSecondsAsync(0.05f);
                _pressAnyKeyLabel.style.color = new StyleColor(new Color(1, 1, 1, 1));
                await Awaitable.WaitForSecondsAsync(0.05f);
            }

            _pressAnyKeyLabel.style.color = new StyleColor(new Color(0, 0, 0, 0));

            _mainMenuElement.RemoveFromClassList("down");

            _startButton.Focus();
        }

        private void Start() {
            FMODManager.RaiseSetParameterEvent("levelMusicProgression", 0);
            FMODManager.RaiseMusicEvent("Music_level1");

            _cameraEffects = ServiceLocator.Get<CameraEffects>();
        }

        private void StartButtonClicked() {
            _cameraEffects.CircleFadeOut(1, 
                () => _ = SceneCollectionManager.LoadSceneCollectionAsync(startSceneCollectionSO, 
                    () => _cameraEffects.CircleFadeIn(1)));
            
        }

        private void Update() {
            _updateActions.Invoke();
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