using System;
using Attributes;
using SceneCollections;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
    [RequireComponent(typeof(UITK_WorldSpaceInteractive))]
    public class SavesMenu : MonoBehaviour {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        [SerializeField, Required] private SaveSlotData saveSlot1Data;
        [SerializeField, Required] private SaveSlotData saveSlot2Data;
        [SerializeField, Required] private SaveSlotData saveSlot3Data;
        
        private event Action OnMenuActivated = delegate { };
        private event Action OnMenuDeactivated = delegate { };
        
        #region ---PrivateFields---
        private VisualElement _root;
        private VisualElement _savesMenuRoot;

        private CameraEffects _cameraEffects;
        private Button _saveSlot1;
        private Button _saveSlot2;
        private Button _saveSlot3;
        #endregion
        #endregion

        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _savesMenuRoot = _root.Q<VisualElement>("SavesMenu");
        }

        private void Start() {
            _saveSlot1 = _savesMenuRoot.Q<Button>("SaveSlot1");
            _saveSlot2 = _savesMenuRoot.Q<Button>("SaveSlot2");
            _saveSlot3 = _savesMenuRoot.Q<Button>("SaveSlot3");

            _saveSlot1.clicked += () => LoadSave(saveSlot1Data);
            _saveSlot2.clicked += () => LoadSave(saveSlot2Data);
            _saveSlot3.clicked += () => LoadSave(saveSlot3Data);
        }

        private void LoadSave(SaveSlotData saveSlotData) {
            _cameraEffects.CircleFadeOut(1,
                () => _ = SceneCollectionManager.LoadSceneCollectionAsync(saveSlotData.currentSceneCollection,
                    () => _cameraEffects.CircleFadeIn(1)));
        }
        
        public void SetOnActivated(Action action) {
            OnMenuActivated = action;
        }
        
        public void SetOnDeactivated(Action action) {
            OnMenuDeactivated = action;
        }

        public void Activate() {
            _saveSlot1.Focus();
            
            Debug.Log("SavesMenu Activated");
            
            OnMenuActivated.Invoke();
        }

        public void Deactivate() {
            Debug.Log("SavesMenu Deactivated");
            
            OnMenuDeactivated.Invoke();
        }
    }
}