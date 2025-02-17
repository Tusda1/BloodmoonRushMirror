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
    [SerializeField, Required] private SceneCollectionSO quitLoadCollection;

    #region ***---UIDocument---***
    private VisualElement _root;
    
    private Button _retryButton;
    private Button _quitButton;
    #endregion

    private CameraEffects _cameraEffects;
    private IAudioManager _audioManager;

    private const string GameOverMusic_SoundID = "Music_GameOver";
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
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
        
        _retryButton.clicked += () => {
            _cameraEffects.CircleFadeOut(0.5f);
            _ = SceneCollectionManager.LoadSceneCollectionAsync(retryLoadCollection, () =>
                _cameraEffects.CircleFadeIn(0.5f));
        };
        
        _quitButton.clicked += () => {
            _cameraEffects.CircleFadeOut(0.5f);
            _ = SceneCollectionManager.LoadSceneCollectionAsync(quitLoadCollection, () =>
                _cameraEffects.CircleFadeIn(0.5f));
        };
        
        _retryButton.Focus();
    }
}