using Attributes;
using Consensus;
using SceneCollections;
using UI.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;
using Cursor = UnityEngine.Cursor;

namespace UI {
    [RequireComponent(typeof(SettingsMenu), typeof(UITKWorldSpaceInteractive))]
    public class PauseMenu : MonoBehaviour {
        #region ---Fields---
        #region ***---GetOrAddComponents---***
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        [GetOrAddComponent, SerializeField, HideInInspector] private SettingsMenu settingsMenu;
        #endregion

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

        private readonly ScriptVotePair _currentVote = new();
        #endregion

        #region ***---DI---***
        [Inject] private IPauseManager _pauseManager;
        [Inject] private EventSystem _eventSystem;
        #endregion
        #endregion

        #region ---EventFunctions---
        private void Start() {
            _root = uiDocument.rootVisualElement;
            _pauseMenuRoot = _root.Q<VisualElement>("PauseMenuRoot");

            _resumeButton = _pauseMenuRoot.Q<Button>("ResumeButton");
            _settingsButton = _pauseMenuRoot.Q<Button>("SettingsButton");
            _mainMenuButton = _pauseMenuRoot.Q<Button>("MainMenuButton");

            pauseAction.action.performed += PauseToggle;

            _resumeButton.clicked += _pauseManager.Resume;
            _settingsButton.clicked += settingsMenu.Activate;
            _mainMenuButton.clicked += () =>
                _ = SceneCollectionManager.LoadSceneCollectionAsync(mainMenusSceneCollectionSO);

            settingsMenu.OnMenuActivated += HideMenu;
            settingsMenu.OnMenuDeactivated += ShowMenu;

            _pauseManager.OnResume += ResumeGame;
            _pauseManager.OnPause += PauseGame;

            ResumeGame();
        }

        private void OnDestroy() {
            _pauseManager.OnResume -= ResumeGame;
            _pauseManager.OnPause -= PauseGame;
            pauseAction.action.performed -= PauseToggle;
        }
        #endregion

        private void PauseToggle(InputAction.CallbackContext callbackContext) {
            _pauseManager.Paused = !_pauseManager.Paused;
        }

        [ContextMenu(nameof(PauseGame))]
        private void PauseGame() {
            _root.style.display = DisplayStyle.Flex;
            _root.enabledSelf = true;
            playerCanMove.ChangeVote(_currentVote, false);

            _eventSystem.SetSelectedGameObject(gameObject);
            _resumeButton.Focus();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        [ContextMenu(nameof(ResumeGame))]
        private void ResumeGame() {
            _root.style.display = DisplayStyle.None;
            _root.enabledSelf = false;
            playerCanMove.ChangeVote(_currentVote, true);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        [ContextMenu(nameof(HideMenu))]
        private void HideMenu() {
            _pauseMenuRoot.AddToClassList("left");
            _pauseMenuRoot.enabledSelf = false;
        }

        [ContextMenu(nameof(ShowMenu))]
        private void ShowMenu() {
            _pauseMenuRoot.enabledSelf = true;
            _pauseMenuRoot.RemoveFromClassList("left");
        }
    }
}