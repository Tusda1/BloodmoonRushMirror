using System.Collections;
using Attributes;
using Consensus;
using SceneCollections;
using UI_Toolkit;
using UI.Settings;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UI {
    [RequireComponent(typeof(SettingsMenu), typeof(UITK_WorldSpaceInteractive))]
    public class PauseMenu : Singleton<PauseMenu> {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        [GetOrAddComponent, SerializeField, HideInInspector] private SettingsMenu settingsMenu;
        
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

            _resumeButton = _root.Q<SpriteButton>("ResumeButton");
            _settingsButton = _root.Q<SpriteButton>("SettingsButton");
            _mainMenuButton = _root.Q<SpriteButton>("MainMenuButton");

            _resumeButton.clicked += ResumeGame;
            _settingsButton.clicked += settingsMenu.Activate;
            _mainMenuButton.clicked += () => _ = SceneCollectionManager.LoadSceneCollectionAsync(mainMenusSceneCollectionSO);

            settingsMenu.SetOnActivated(() => {
                _pauseMenuRoot.AddToClassList("left");
                _pauseMenuRoot.enabledSelf = false;
            });

            settingsMenu.SetOnDeactivated(() => {
                _pauseMenuRoot.RemoveFromClassList("left");
                _pauseMenuRoot.enabledSelf = true;
            });

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
        }
        #endregion
    }
}