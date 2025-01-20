using Attributes;
using SceneCollections;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class GameOverUI : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
    [SerializeField, Required] private SceneCollectionSO retryLoadCollection, quitLoadCollection;
    
    private VisualElement _root;
    private Button _retryButton;
    private Button _quitButton;
    
    [Inject] private CameraEffects _cameraEffects;
    #endregion
    
    private void Start() {
        _root = uiDocument.rootVisualElement;
        
        _retryButton = _root.Q<Button>("RetryButton");
        _quitButton = _root.Q<Button>("QuitButton");

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