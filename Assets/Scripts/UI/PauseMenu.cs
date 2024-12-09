using System.Collections;
using Attributes;
using Consensus;
using UI.Settings;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UI {
    [RequireComponent(typeof(SettingsMenu), typeof(UITK_WorldSpaceInteractive))]
    public class PauseMenu : Singleton<PauseMenu> {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector]
        private UIDocument uiDocument;
        
        [SerializeField, Required] private SceneCollectionSO mainMenusSceneCollectionSO;
        [SerializeField, Required] private AllInFavourConsensusSO playerCanMove;
        

        #region ***---PrivateFields---***
        private VisualElement _root;
        private VisualElement _pauseMenuRoot;
        private VisualElement _settingsMenuRoot;

        private Button _resumeButton;
        private Button _settingsButton;
        private Button _mainMenuButton;
        private Button _backButton;

        private bool _paused;
        
        private EventSystem _eventSystem;

        private readonly ScriptVotePair _currentVote = new();
        #endregion
        #endregion

        private IEnumerator Start() {
            _root = uiDocument.rootVisualElement;
            _pauseMenuRoot = _root.Q<VisualElement>("PauseMenuRoot");
            _settingsMenuRoot = _root.Q<VisualElement>("SettingsMenuRoot");

            _resumeButton = _root.Q<Button>("ResumeButton");
            _settingsButton = _root.Q<Button>("SettingsButton");
            _mainMenuButton = _root.Q<Button>("MainMenuButton");
            _backButton = _root.Q<Button>("BackButton");

            _resumeButton.clicked += ResumeGame;
            _settingsButton.clicked += SettingsButtonPressed;
            _mainMenuButton.clicked += () => _ = SceneCollectionManager.LoadSceneCollectionAsync(mainMenusSceneCollectionSO);
            _backButton.clicked += BackButtonPressed;

            _eventSystem ??= GameObject.FindWithTag("EventSystem").GetComponent<EventSystem>();
            
            Assert.IsNotNull(_eventSystem, "EventSystem reference is not set!");

            yield return null;
            yield return null;
            yield return null;
            
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
            _settingsMenuRoot.enabledSelf = true;
            _settingsMenuRoot.RemoveFromClassList("right");
        }

        private void BackButtonPressed() {
            _pauseMenuRoot.enabledSelf = true;
            _pauseMenuRoot.RemoveFromClassList("left");
            _settingsMenuRoot.AddToClassList("right");
            _settingsMenuRoot.enabledSelf = false;
        }

        [ContextMenu(nameof(PauseGame))]
        private void PauseGame() {
            Time.timeScale = 0;
            _paused = true;

            _root.style.display = DisplayStyle.Flex;
            _root.enabledSelf = true;
            playerCanMove.ChangeVote(_currentVote, false);
            
            _eventSystem.SetSelectedGameObject(gameObject);
            _resumeButton.Focus();
        }

        [ContextMenu(nameof(ResumeGame))]
        private void ResumeGame() {
            Time.timeScale = 1;
            _paused = false;

            _root.style.display = DisplayStyle.None;
            _root.enabledSelf = false;
            playerCanMove.ChangeVote(_currentVote, true);

            BackButtonPressed();
        }
        #endregion
    }
}