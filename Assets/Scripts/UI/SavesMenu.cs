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
        private ISaveLoadSystemManager _saveLoadSystemManager;
        private CameraEffects _cameraEffects;
        private SaveSlots _saveSlots;
        #endregion
        #endregion

        #region ---Initialization---
        [Inject]
        public void Construct(IObjectResolver container) {
#if SAVESYSTEM_ACTIVE
            _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
#endif
            _cameraEffects = container.Resolve<CameraEffects>();
            _saveSlots = container.Resolve<SaveSlots>();
        }
        
        private void Awake() {
            _root = uiDocument.rootVisualElement;
            _savesMenuRoot = _root.Q<VisualElement>("SavesMenuRoot");
            
            _saveSlot1 = _savesMenuRoot.Q<Button>("SaveSlot1");
            _saveSlot2 = _savesMenuRoot.Q<Button>("SaveSlot2");
            _saveSlot3 = _savesMenuRoot.Q<Button>("SaveSlot3");
        }

        private void Start() {
#if SAVESYSTEM_ACTIVE
            _saveLoadSystemManager.UpdateSaveSlotSO();
#endif
            _saveSlot1.clicked += () => LoadSave(_saveSlots.saveSlot1Data);
            _saveSlot2.clicked += () => LoadSave(_saveSlots.saveSlot2Data);
            _saveSlot3.clicked += () => LoadSave(_saveSlots.saveSlot3Data);
            
            _savesMenuRoot.SetEnabled(false);
        }

        private void OnDisable() {
            backAction.action.performed -= Deactivate;
        }
        #endregion
        
        #region ---IMenu---
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
        #endregion

        #region ---PrivateMethods---
        private void Deactivate(InputAction.CallbackContext obj) {
            Deactivate();
        }
        
        private void LoadSave(SaveSlotDataSO saveSlotData) {
#if SAVESYSTEM_ACTIVE
            if (saveSlotData.lastTimePlayed == string.Empty) {
                _saveLoadSystemManager.SetCurrentGameDataFromTemplate(saveSlotData.name);
                saveSlotData.currentSceneCollection = newGameSceneCollection;
            }
            else {
                _saveLoadSystemManager.SetCurrentGameDataFromSaveSlot(saveSlotData);
                saveSlotData.currentSceneCollection = _saveLoadSystemManager.CurrentGameData.CurrentSceneCollection;
            }

            Debug.Log(saveSlotData.currentSceneCollection.name);

            _cameraEffects.FadeLoadScene(saveSlotData.currentSceneCollection);
#endif
        }
        #endregion
    }
}