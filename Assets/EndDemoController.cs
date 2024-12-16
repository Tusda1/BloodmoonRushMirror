using Attributes;
using SceneCollections;
using UnityEngine;
using UnityEngine.UIElements;

public class EndDemoController : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] 
    private UIDocument uiDocument;
    
    [SerializeField] private SceneCollectionSO mainMenu;
    
    private CameraEffects _cameraEffects;
    private VisualElement _root;
    #endregion

    private void Awake() {
        _root = uiDocument.rootVisualElement;
    }

    private void Start() {
        _cameraEffects = ServiceLocator.Get<CameraEffects>();

        _cameraEffects.CircleFadeIn(1);
        
        var backToMainMenuButton = _root.Q<Button>("BackToMainMenuButton");
        backToMainMenuButton.clicked += () => {
            _cameraEffects.CircleFadeOut(1,
                () => _ = SceneCollectionManager.LoadSceneCollectionAsync(mainMenu));
        };
    }
}