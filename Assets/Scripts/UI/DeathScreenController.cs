using Attributes;
using SceneCollections;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UI {
    public class DeathScreenController : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        [SerializeField, Required] private SceneCollectionSO mainMenuSceneCollection;
        
        private Button _retryButton;
        private Button _mainMenuButton;
        
        [Inject] private IAudioManager _audioManager;
        #endregion
        
        private void Start() {
            _audioManager.StopAllMusic();

            var root = uiDocument.rootVisualElement;
            
            _retryButton = root.Q<Button>("YesButton");
            _mainMenuButton = root.Q<Button>("NoButton");
            
            // TODO: 
            // _retryButton.clicked += () => _ = SceneCollectionManager.LoadSceneCollectionAsync();
            _mainMenuButton.clicked += () => _ = SceneCollectionManager.LoadSceneCollectionAsync(mainMenuSceneCollection);
        }
    }
}