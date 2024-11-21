using Attributes;
using UI.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    [RequireComponent(typeof(SettingsMenu))]
    public class PauseMenu : Singleton<PauseMenu> {
        #region ---Fields---
        [GetOrAddComponent] private UIDocument _uiDocument;
        
        [SerializeField] private SceneCollectionSO mainMenusSceneCollectionSO;
        
        private VisualElement _root;
        private VisualElement _pauseMenuRoot;
        private VisualElement _settingsMenuRoot;
        
        private Button _resumeButton;
        private Button _settingsButton;
        private Button _mainMenuButton;
        private Button _backButton;
        
        private bool _paused;
        #endregion

        protected override void Awake() {
            base.Awake();

            _root = _uiDocument.rootVisualElement;
            _pauseMenuRoot = _root.Q<VisualElement>("PauseMenuRoot");
            _settingsMenuRoot = _root.Q<VisualElement>("SettingsMenuRoot");

            _resumeButton = _root.Q<Button>("ResumeButton");
            _settingsButton = _root.Q<Button>("SettingsButton");
            _mainMenuButton = _root.Q<Button>("MainMenuButton");
            _backButton = _root.Q<Button>("BackButton");

            _resumeButton.clicked += ResumeGame;
            _settingsButton.clicked += SettingsButtonPressed;
            _mainMenuButton.clicked += () => _ = SceneCollectionManager.LoadSceneCollection(mainMenusSceneCollectionSO);
            _backButton.clicked += BackButtonPressed;

            ResumeGame();
        }

        public void PauseToggle() {
            if (_paused) {
                ResumeGame();
            }
            else {
                PauseGame();
            }
        }

        #region ---Private Methods---
        private void SettingsButtonPressed() {
            _pauseMenuRoot.AddToClassList("left");
            _pauseMenuRoot.enabledSelf = false;
            _settingsMenuRoot.RemoveFromClassList("right");
            _settingsMenuRoot.enabledSelf = true;
        }

        private void BackButtonPressed() {
            _pauseMenuRoot.RemoveFromClassList("left");
            _pauseMenuRoot.enabledSelf = true;
            _settingsMenuRoot.AddToClassList("right");
            _settingsMenuRoot.enabledSelf = false;
        }

        private void PauseGame() {
            // Time.timeScale = 0;
            _paused = true;

            _root.style.display = DisplayStyle.Flex;
            _root.enabledSelf = true;
        }

        private void ResumeGame() {
            Time.timeScale = 1;
            _paused = false;

            _root.style.display = DisplayStyle.None;
            _root.enabledSelf = false;

            BackButtonPressed();
        }
        #endregion
    }
}