using Attributes;
using SceneCollections;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

[RequireComponent(typeof(UITKWorldSpaceInteractive))]
public class GameOverUI : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;

    [SerializeField, Required] private SceneCollectionSO retryLoadCollection;

    #region ***---UIDocument---***
    private VisualElement _root;

    private Button _retryButton;
    private Button _quitButton;
    #endregion

    private ISaveLoadSystemManager _saveLoadSystemManager;
    private CameraEffects _cameraEffects;
    private IAudioManager _audioManager;

    private const string GameOverMusic_SoundID = "Music_GameOver";
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
#if SAVESYSTEM_ACTIVE
        _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
#endif
        _cameraEffects = container.Resolve<CameraEffects>();
        _audioManager = container.Resolve<IAudioManager>();
    }

    private void Awake() {
        _root = uiDocument.rootVisualElement;

        _retryButton = _root.Q<Button>("RetryButton");
        _quitButton = _root.Q<Button>("QuitButton");
    }

    private void Start() {
        _audioManager.PlayMusic(GameOverMusic_SoundID);

        _cameraEffects.CircleFadeIn(0.5f);

        _retryButton.clicked += () => {
#if SAVESYSTEM_ACTIVE
            _cameraEffects.CircleFadeOut(0.5f);
            _ =
 SceneCollectionManager.LoadSceneCollectionAsync(_saveLoadSystemManager.CurrentGameData.CurrentSceneCollection,
                () => {
                    _saveLoadSystemManager.SaveLoad();
                    _cameraEffects.CircleFadeIn(0.5f);
                });
#else
            _cameraEffects.CircleFadeOut(0.5f);
            _ = SceneCollectionManager.LoadSceneCollectionAsync(retryLoadCollection, () =>
                _cameraEffects.CircleFadeIn(0.5f));
#endif
        };

        _quitButton.clicked += Application.Quit;

        _retryButton.Focus();
    }
}