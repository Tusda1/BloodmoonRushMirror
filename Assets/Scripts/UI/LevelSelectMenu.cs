using System;
using Attributes;
using SceneCollections;
using UI.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VContainer;

namespace UI {
    [RequireComponent(typeof(UITKWorldSpaceInteractive))]
    public class LevelSelectMenu : MonoBehaviour, IMenu {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        [SerializeField] private SceneCollectionSO[] scenes;
        
        [SerializeField, Required] private InputActionReference backAction;
        
        public event Action OnMenuActivated = delegate { };
        public event Action OnMenuDeactivated = delegate { };
        
        private VisualElement _root;
        private VisualElement _levelSelectMenuRoot;
        private VisualElement _levelSelectButtonContainer;
        
        [Inject] private CameraEffects _cameraEffects;
        #endregion

        #region ---EventFunctions---
        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _levelSelectMenuRoot = _root.Q<VisualElement>("LevelSelectMenuRoot");
            _levelSelectButtonContainer = _root.Q<VisualElement>("LevelSelectButtonContainer");
        }

        private void Start() {
            foreach (var sceneCollectionSO in scenes) {
                CreateLevelSelectButton(sceneCollectionSO);
            }
            
            _levelSelectMenuRoot.AddToClassList("left");
            _levelSelectMenuRoot.enabledSelf = false;
        }

        private void OnDisable() {
            backAction.action.performed -= Deactivate;
        }
        #endregion

        #region ---IMenu---
        public void Activate() {
            _levelSelectMenuRoot.enabledSelf = true;
            _levelSelectMenuRoot.RemoveFromClassList("left");
            
            backAction.action.performed += Deactivate;
            
            OnMenuActivated.Invoke();
        }

        public void Deactivate(string addToClassList = null) {
            _levelSelectMenuRoot.AddToClassList("left");
            
            backAction.action.performed -= Deactivate;

            _levelSelectMenuRoot.enabledSelf = false;
            
            OnMenuDeactivated.Invoke();
        }
        #endregion
        
        private void Deactivate(InputAction.CallbackContext obj) {
            Deactivate();
        }

        private void CreateLevelSelectButton(SceneCollectionSO scene) {
            var button = new Button(() => LoadLevel(scene)) {
                text = scene.name
            };
            
            _levelSelectButtonContainer.Add(button);
        }

        private void LoadLevel(SceneCollectionSO scene) {
            _levelSelectMenuRoot.enabledSelf = false;
            
            _cameraEffects.CircleFadeOut(1,
                () => _ = SceneCollectionManager.LoadSceneCollectionAsync(scene,
                    () => _cameraEffects.CircleFadeIn(1)));
        }
    }
}