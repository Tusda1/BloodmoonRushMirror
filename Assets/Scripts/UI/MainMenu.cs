using System;
using Attributes;
using Consensus;
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
    [RequireComponent(typeof(UIDocument), typeof(SettingsMenu), typeof(SavesMenu))]
    public class MainMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        [GetOrAddComponent, SerializeField, HideInInspector] private SettingsMenu settingsMenu;
        [GetOrAddComponent, SerializeField, HideInInspector] private SavesMenu savesMenu;

        [SerializeField, Required] private SceneCollectionSO startSceneCollectionSO;
        [SerializeField, Required] private Animator backgroundAnimator;
        [SerializeField] private AllInFavourConsensusSO playerCanMove;

        #region ---PrivateFields---
        private VisualElement _rootElement;
        private VisualElement _mainMenuRoot;

        private Label _pressAnyKeyLabel;

        private Button _startButton;
        private Button _settingsButton;
        private Button _quitButton;
        
        private Action _updateActions = delegate { };
        #endregion
        #endregion

        private void Awake() {
            _rootElement = uiDocument.rootVisualElement;

            _mainMenuRoot = _rootElement.Q<VisualElement>("MainMenu");
            _pressAnyKeyLabel = _rootElement.Q<Label>("PressAnyKeyLabel");

            _startButton = _rootElement.Q<Button>("StartButton");
            _settingsButton = _rootElement.Q<Button>("SettingsButton");
            _quitButton = _rootElement.Q<Button>("QuitButton");

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            Time.timeScale = 1;
            playerCanMove.Clear();

            _updateActions += () => {
                if (Input.anyKeyDown) _ = AnyKeyPressed();
            };
        }

        private void Start() {
            FMODManager.RaiseSetParameterEvent("levelMusicProgression", 0);
            FMODManager.RaiseMusicEvent("Music_level1");
            
            _startButton.clicked += savesMenu.Activate;
            savesMenu.SetOnActivated(HideMenu);
            savesMenu.SetOnDeactivated(ShowMenu);
            
            _settingsButton.clicked += settingsMenu.Activate;
            settingsMenu.SetOnActivated(HideMenu);
            settingsMenu.SetOnDeactivated(ShowMenu);
            
            _quitButton.clicked += QuitButtonClicked;
        }

        private void Update() {
            _updateActions.Invoke();
        }

        private void HideMenu() {
            _mainMenuRoot.AddToClassList("down");
            _mainMenuRoot.SetEnabled(false);
        }
        
        private void ShowMenu() {
            _mainMenuRoot.SetEnabled(true);
            _mainMenuRoot.RemoveFromClassList("down");
            
            _startButton.Focus();
        }

        private static void QuitButtonClicked() {
            Debug.Log("QuitButtonClicked");
            
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
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

            ShowMenu();
        }
    }
}