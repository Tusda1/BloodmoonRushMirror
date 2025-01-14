using System;
using Attributes;
using SaveLoadSystem;
using SceneCollections;
using UI.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UI {
    [RequireComponent(typeof(UITKWorldSpaceInteractive))]
    public class SavesMenu : MonoBehaviour, IMenu {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        [SerializeField, Required] private SaveSlotDataSO saveSlot1Data;
        [SerializeField, Required] private SaveSlotDataSO saveSlot2Data;
        [SerializeField, Required] private SaveSlotDataSO saveSlot3Data;
        
        [SerializeField, Required] private SceneCollectionSO newGameSceneCollection;
        
        [SerializeField, Required] private InputActionReference backAction;
        
        public event Action OnMenuActivated = delegate { };
        public event Action OnMenuDeactivated = delegate { };
        
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
            _savesMenuRoot = _root.Q<VisualElement>("SavesMenuRoot");
            
            _saveSlot1 = _savesMenuRoot.Q<Button>("SaveSlot1");
            _saveSlot2 = _savesMenuRoot.Q<Button>("SaveSlot2");
            _saveSlot3 = _savesMenuRoot.Q<Button>("SaveSlot3");
        }

        private void Start() {
            _cameraEffects = ServiceLocator.Get<CameraEffects>();

            _saveSlot1.clicked += () => LoadSave(saveSlot1Data);
            _saveSlot2.clicked += () => LoadSave(saveSlot2Data);
            _saveSlot3.clicked += () => LoadSave(saveSlot3Data);
            
            _savesMenuRoot.SetEnabled(false);
        }

        private void OnDisable() {
            backAction.action.performed -= Deactivate;
        }

        private void LoadSave(SaveSlotDataSO saveSlotData) {
            if (saveSlotData.lastTimePlayed == string.Empty) {
                SaveLoadSystemManager.Instance.NewGame(saveSlotData);
                saveSlotData.currentSceneCollection = newGameSceneCollection;
            }

            Debug.Log(saveSlotData.currentSceneCollection.name);
            
            _cameraEffects.CircleFadeOut(1,
                () => _ = SceneCollectionManager.LoadSceneCollectionAsync(saveSlotData.currentSceneCollection,
                    () => _cameraEffects.CircleFadeIn(1)));
        }

        public void Activate() {
            _saveSlot1.Focus();
            _savesMenuRoot.SetEnabled(true);
            _savesMenuRoot.RemoveFromClassList("left");
            backAction.action.performed += Deactivate;
            
            Debug.Log("SavesMenu Activated");
            
            OnMenuActivated.Invoke();
        }

        public void Deactivate(string addToClassList = null) {
            _savesMenuRoot.AddToClassList("left");
            backAction.action.performed -= Deactivate;
            _savesMenuRoot.SetEnabled(false);
            Debug.Log("SavesMenu Deactivated");
            
            OnMenuDeactivated.Invoke();
        }

        private void Deactivate(InputAction.CallbackContext obj) {
            Deactivate();
        }
    }
}