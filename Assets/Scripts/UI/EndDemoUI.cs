using Attributes;
using SceneCollections;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

public class EndDemoUI : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
    
    [SerializeField] private SceneCollectionSO mainMenu;
    
    #region ***---UIDocument---***
    private VisualElement _root;
    private Button _backToMainMenuButton;
    #endregion
    
    private CameraEffects _cameraEffects;
    private IAudioManager _audioManager;

    private const string EndDemoMusic_SoundID = "Music_EndDemo";
    #endregion
    
    [Inject]
    public void Construct(IObjectResolver container) {
        _cameraEffects = container.Resolve<CameraEffects>();
        _audioManager = container.Resolve<IAudioManager>();
    }

    private void Awake() {
        _root = uiDocument.rootVisualElement;
    }

    private void Start() {
        _audioManager.SetParameterValue(LevelMusicManager.CalmnessParameter, 1, true);
        _audioManager.PlayMusic(EndDemoMusic_SoundID);
        
        _cameraEffects.CircleFadeIn(1);
        
        _backToMainMenuButton = _root.Q<Button>("BackToMainMenuButton");
        _backToMainMenuButton.clicked += () => {
            _cameraEffects.CircleFadeOut(1,
                () => _ = SceneCollectionManager.LoadSceneCollectionAsync(mainMenu,
                    () => _cameraEffects.CircleFadeIn(1)));
        };
        
        _backToMainMenuButton.Focus();
    }
}