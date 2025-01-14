using Attributes;
using Consensus;
using SceneCollections;
using UI_Toolkit;
using UI.Settings;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UI {
    [RequireComponent(typeof(SettingsMenu), typeof(UITKWorldSpaceInteractive))]
    public class PauseMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        [GetOrAddComponent, SerializeField, HideInInspector] private SettingsMenu settingsMenu;
        
        [SerializeField, Required] private SceneCollectionSO mainMenusSceneCollectionSO;
        [SerializeField, Required] private AllInFavourConsensusSO playerCanMove;
        
        [SerializeField, Required] private InputActionReference pauseAction;

        #region ***---PrivateFields---***
        private VisualElement _root;
        private VisualElement _pauseMenuRoot;

        private Button _resumeButton;
        private Button _settingsButton;
        private Button _mainMenuButton;
        private Button _backButton;

        private bool _paused;
        
        private EventSystem _eventSystem;

        private readonly ScriptVotePair _currentVote = new();
        #endregion
        #endregion

        private void Start() {
            _root = uiDocument.rootVisualElement;
            _pauseMenuRoot = _root.Q<VisualElement>("PauseMenuRoot");

            _resumeButton = _pauseMenuRoot.Q<SpriteButton>("ResumeButton");
            _settingsButton = _pauseMenuRoot.Q<SpriteButton>("SettingsButton");
            _mainMenuButton = _pauseMenuRoot.Q<SpriteButton>("MainMenuButton");
            
            pauseAction.action.performed += PauseToggle;
            
            _resumeButton.clicked += ResumeGame;
            _settingsButton.clicked += settingsMenu.Activate;
            _mainMenuButton.clicked += () => _ = SceneCollectionManager.LoadSceneCollectionAsync(mainMenusSceneCollectionSO);
            
            settingsMenu.OnMenuActivated += HideMenu;
            settingsMenu.OnMenuDeactivated += ShowMenu;
            
            _eventSystem ??= GameObject.FindWithTag("EventSystem").GetComponent<EventSystem>();
            
            Assert.IsNotNull(_eventSystem, "EventSystem reference is not set!");
            
            ResumeGame();
        }
        
        private void PauseToggle(InputAction.CallbackContext callbackContext) {
            if (_paused) {
                ResumeGame();
            }
            else {
                PauseGame();
            }
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
        }

        [ContextMenu(nameof(HideMenu))]
        private void HideMenu() {
            _pauseMenuRoot.AddToClassList("left");
            _pauseMenuRoot.enabledSelf = false;
        }

        [ContextMenu(nameof(ShowMenu))]
        private void ShowMenu() {
            _pauseMenuRoot.RemoveFromClassList("left");
            _pauseMenuRoot.enabledSelf = true;
        }
    }
}