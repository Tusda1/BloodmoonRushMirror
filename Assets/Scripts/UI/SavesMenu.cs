using System;
using Attributes;
using SaveLoadSystem;
using SceneCollections;
using UI.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

namespace UI {
    [RequireComponent(typeof(UITKWorldSpaceInteractive))]
    public class SavesMenu : MonoBehaviour, IMenu {
        #region ---Fields---
        [GetOrAddComponent, SerializeField, HideInInspector] private UIDocument uiDocument;
        
        [SerializeField, Required] private SceneCollectionSO newGameSceneCollection;
        
        [SerializeField, Required] private InputActionReference backAction;
        
        public event Action OnMenuActivated = delegate { };
        public event Action OnMenuDeactivated = delegate { };
        
        #region ---PrivateFields---
        private VisualElement _root;
        private VisualElement _savesMenuRoot;

        private Button _saveSlot1;
        private Button _saveSlot2;
        private Button _saveSlot3;
        #endregion

        #region ***---DI---***
        [Inject] private CameraEffects _cameraEffects;
        [Inject] private SaveLoadSystemManager _saveLoadSystemManager;
        
        [Inject(Id = "Slot1")] private SaveSlotDataSO _saveSlot1Data;
        [Inject(Id = "Slot2")] private SaveSlotDataSO _saveSlot2Data;
        [Inject(Id = "Slot3")] private SaveSlotDataSO _saveSlot3Data;
        #endregion
        #endregion

        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _savesMenuRoot = _root.Q<VisualElement>("SavesMenuRoot");
            
            _saveSlot1 = _savesMenuRoot.Q<Button>("SaveSlot1");
            _saveSlot2 = _savesMenuRoot.Q<Button>("SaveSlot2");
            _saveSlot3 = _savesMenuRoot.Q<Button>("SaveSlot3");

            _saveLoadSystemManager.UpdateSaveSlotSO();
        }

        private void Start() {
            _saveSlot1.clicked += () => LoadSave(_saveSlot1Data);
            _saveSlot2.clicked += () => LoadSave(_saveSlot2Data);
            _saveSlot3.clicked += () => LoadSave(_saveSlot3Data);
            
            _savesMenuRoot.SetEnabled(false);
        }

        private void OnDisable() {
            backAction.action.performed -= Deactivate;
        }

        private void LoadSave(SaveSlotDataSO saveSlotData) {
            if (saveSlotData.lastTimePlayed == string.Empty) {
                _saveLoadSystemManager.NewGame(saveSlotData);
                saveSlotData.currentSceneCollection = newGameSceneCollection;
            }
            else {
                _saveLoadSystemManager.LoadSave(saveSlotData.saveSlotName);
                saveSlotData.currentSceneCollection = _saveLoadSystemManager.gameData.CurrentSceneCollection;
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