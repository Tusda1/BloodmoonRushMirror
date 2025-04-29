using System;
using Attributes;
using Consensus;
using SceneCollections;
using UI.Settings;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using Cursor = UnityEngine.Cursor;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI {
    [RequireComponent(typeof(UITKWorldSpaceInteractive))]
    public class MainMenu : MonoBehaviour {
        #region ---Fields---
        #region ***---GetOrAddComponents---***
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        [GetOrAddComponent, SerializeField, HideInInspector] private SettingsMenu settingsMenu;
        [GetOrAddComponent, SerializeField, HideInInspector] private SavesMenu savesMenu;
        [GetOrAddComponent, SerializeField, HideInInspector] private LevelSelectMenu levelSelectMenu;
        #endregion

        [SerializeField, Required] private SceneCollectionSO firstSceneCollection;
        [SerializeField, Required] private Animator backgroundAnimator;
        [SerializeField] private AllInFavourConsensusSO playerCanMove;
        [SerializeField] private Animator colorSwitcherAnimator;

        #region ***---PrivateFields---***
        private VisualElement _rootElement;
        private VisualElement _mainMenuRoot;

        private Label _pressAnyKeyLabel;

        private Button _startButton;
        private Button _settingsButton;
        private Button _quitButton;
        private Button _creditsButton;

        private VisualElement _buttons;

        private IAudioManager _audioManager;
        private DebugLoggerSO _debugLoggerSO;
        private CameraEffects _cameraEffects;
        private ISaveLoadSystemManager _saveLoadSystemManager;

        private Action _updateActions = delegate { };
        
        private bool _creditsActive;
        #endregion

        #region ***---SoundID---***
        private const string MainMenuMusic_SoundID = "Music_MainMenu";
        private const string ShowCredits_SoundID = "UI_ShowCredits";
        
        private const string ButtonDownSFXBus_SoundID = "UI_ButtonDown[SFXBus]";
        
        private const string AnyKeyPressed_SoundID = "Player_Respawn";
        #endregion
        
        private static readonly int CreditsActive = Animator.StringToHash("CreditsActive");
        #endregion

        #region ---Initialization---
        [Inject]
        public void Construct(IObjectResolver container) {
            _audioManager = container.Resolve<IAudioManager>();
            _debugLoggerSO = container.ResolveProviderID<DebugLoggerSO>("UI");
            _cameraEffects = container.Resolve<CameraEffects>();
            _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
        }

        private void Awake() {
            _rootElement = uiDocument.rootVisualElement;

            _mainMenuRoot = _rootElement.Q<VisualElement>("MainMenuRoot");
            _pressAnyKeyLabel = _rootElement.Q<Label>("PressAnyKeyLabel");

            _startButton = _rootElement.Q<Button>("StartButton");
            _settingsButton = _rootElement.Q<Button>("SettingsButton");
            _quitButton = _rootElement.Q<Button>("QuitButton");
            _creditsButton = _rootElement.Q<Button>("CreditsButton");
            
            _buttons = _mainMenuRoot.Q<VisualElement>("Buttons");

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Time.timeScale = 1;
            playerCanMove.Clear();

            _updateActions += () => {
                if (Input.anyKeyDown) _ = AnyKeyPressed();
            };
        }

        private void Start() {
            _audioManager.PlayMusic(MainMenuMusic_SoundID);
            _audioManager.SetParameterValue(LevelMusicManager.CalmnessParameter, 0, true);

            _saveLoadSystemManager.CurrentGameData = null;

            levelSelectMenu.OnMenuActivated += HideMenu;
            levelSelectMenu.OnMenuDeactivated += ShowMenu;

            savesMenu.OnMenuActivated += HideMenu;
            savesMenu.OnMenuDeactivated += ShowMenu;

            _startButton.clicked += StartButtonClicked;

            _settingsButton.clicked += settingsMenu.Activate;
            settingsMenu.OnMenuActivated += HideMenu;
            settingsMenu.OnMenuDeactivated += ShowMenu;

            _quitButton.clicked += QuitButtonClicked;

            _startButton.RegisterCallback<NavigationMoveEvent>(TriggerFocusCreditsButton);
            _settingsButton.RegisterCallback<NavigationMoveEvent>(TriggerFocusCreditsButton);
            _quitButton.RegisterCallback<NavigationMoveEvent>(TriggerFocusCreditsButton);

            _creditsButton.RegisterCallback<NavigationMoveEvent>(evt => {
                if (evt.direction == NavigationMoveEvent.Direction.Down) {
                    _debugLoggerSO.Log("Navigate Away From Credits Button");
                    
                    _startButton.focusController.IgnoreEvent(evt);
                    evt.StopPropagation();
                    
                    if (_creditsActive) {
                        ExitCredits();
                    }
                }
            });
            _creditsButton.clicked += CreditsButtonClicked;

            _cameraEffects.ResetCameraMove();
        }

        private void CreditsButtonClicked() {
            if (_creditsActive) {
                ExitCredits();
                return;
            }
            
            colorSwitcherAnimator.SetBool(CreditsActive, true);
            _creditsActive = true;
            _buttons.AddToClassList("down");
            
            _audioManager.PlayOneShot(ShowCredits_SoundID);
            _audioManager.SetParameterValue(LevelMusicManager.CalmnessParameter, 1, true);
        }

        private void ExitCredits() {
            colorSwitcherAnimator.SetBool(CreditsActive, false);
            _creditsActive = false;
            _buttons.RemoveFromClassList("down");
                    
            _startButton.Focus();
            _audioManager.SetParameterValue(LevelMusicManager.CalmnessParameter, 0, true);
        }

        private void StartButtonClicked() {
            VirtualCameraSwitch.ActiveVirtualCameraSwitch = null;
#if LEVEL_SELECT
            levelSelectMenu.Activate();
#elif SAVESYSTEM_ACTIVE
            savesMenu.Activate();
#else
            _saveLoadSystemManager.SetCurrentGameDataFromTemplate("NO_LOAD");
            _cameraEffects.FadeLoadScene(firstSceneCollection);

            _startButton.clicked -= StartButtonClicked;
#endif
        }
        #endregion

        private void Update() {
            _updateActions.Invoke();
        }

        private void TriggerFocusCreditsButton(NavigationMoveEvent evt) {
            if (evt.direction == NavigationMoveEvent.Direction.Up) {
                _debugLoggerSO.Log("Navigate To Credits Button");
                
                _startButton.focusController.IgnoreEvent(evt);
                evt.StopPropagation();
                _creditsButton.Focus();
            }
        }

        [ContextMenu(nameof(HideMenu))]
        private void HideMenu() {
            _mainMenuRoot.AddToClassList("down");
            _mainMenuRoot.SetEnabled(false);
            _debugLoggerSO.Log("Hide MainMenu");
            
            _audioManager.PlayOneShot(ButtonDownSFXBus_SoundID);
        }

        [ContextMenu(nameof(ShowMenu))]
        private void ShowMenu() {
            _mainMenuRoot.SetEnabled(true);
            _mainMenuRoot.RemoveFromClassList("down");
            _debugLoggerSO.Log("Show MainMenu");

            _startButton.Focus();
        }

        private void QuitButtonClicked() {
            _debugLoggerSO.Log("QuitButtonClicked");

            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

        private async Awaitable AnyKeyPressed() {
            _updateActions = delegate { };
            
            _debugLoggerSO.Log("Any Key Pressed");

            backgroundAnimator.Play("MenuZoom");
            _audioManager.PlayOneShot(AnyKeyPressed_SoundID);

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