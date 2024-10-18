using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI {
    public class PauseMenu : Singleton<PauseMenu> {
        [SerializeField] private UIDocument uiDocument;

        private bool _paused;

        private VisualElement _root;
        private Button _resumeButton;
        private Button _settingsButton;
        private Button _quitButton;
        private Button _backButton;

        protected override void Awake() {
            base.Awake();

            _root = uiDocument.rootVisualElement;

            _resumeButton = _root.Q<Button>("ResumeButton");
            _settingsButton = _root.Q<Button>("SettingsButton");
            _quitButton = _root.Q<Button>("QuitButton");
            _backButton = _root.Q<Button>("BackButton");

            var pause = _root.Q<VisualElement>("Pause");
            var settings = _root.Q<VisualElement>("Settings");

            _resumeButton.clicked += ResumeGame;
            _settingsButton.clicked += () => {
                settings.RemoveFromClassList("right");
                pause.AddToClassList("left");
            };
            _quitButton.clicked += () => { SceneManager.LoadScene("MainMenu"); };
            _backButton.clicked += () => {
                settings.AddToClassList("right");
                pause.RemoveFromClassList("left");
            };

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

        private void PauseGame() {
            Time.timeScale = 0;
            _paused = true;

            _root.style.display = DisplayStyle.Flex;
            _root.enabledSelf = true;
        }

        private void ResumeGame() {
            Time.timeScale = 1;
            _paused = false;

            _root.style.display = DisplayStyle.None;
            _root.enabledSelf = false;
        }
    }
}