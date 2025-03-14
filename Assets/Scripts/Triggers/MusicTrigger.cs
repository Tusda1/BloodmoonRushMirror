using System;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;
using VContainer;

namespace Triggers {
    public class MusicTrigger : TriggerBox, IBind<TriggerData> {
        #region ---Fields---
        [Header(nameof(MusicTrigger))]
        [SerializeField] private string musicID;
        [SerializeField] private int calmness;

        #region ***---DataBindingVariables---***
        [field: Header("Data Binding")]
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public TriggerData SaveSystemData { get; set; }

        public TriggerData DataInGameData => _levelDataManager.DataInGameData.triggerDatas
            .GetOrAddByIDInList(ID, TriggerDataGenerator);

        private Func<TriggerData> TriggerDataGenerator => () => new TriggerData {
            name = name,
            ID = ID,
            hasBeenTriggered = hasBeenTriggered
        };
        #endregion

        private ISaveLoadSystemManager _saveLoadSystemManager;
        private LevelMusicManager _levelMusicManager;
        private LevelDataManager _levelDataManager;
        #endregion
        
        
        #region ---Initialization---
        [Inject]
        public void Construct(IObjectResolver container) {
#if SAVESYSTEM_ACTIVE
        _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
#endif
            _levelMusicManager = container.Resolve<LevelMusicManager>();
            _levelDataManager = container.Resolve<LevelDataManager>();
        }
        #endregion

        #region ---EventFunctions---
        private void Start() {
            Bind();
        }

        private void OnDestroy() {
            Unbind();
        }
        #endregion

        protected override void TriggeredAction(Collider2D other) {
            _levelMusicManager.SetCalmnessParameter(calmness);
            _levelMusicManager.PlayMusic(musicID);
        }

        #region ---DataBindingMethods---
        public void Bind() {
#if SAVESYSTEM_ACTIVE
            _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
#endif
        }

        public void Unbind() {
#if SAVESYSTEM_ACTIVE
            _saveLoadSystemManager.DataBinder.UnBind(this, DataInGameData);
#endif
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (gameObject.activeInHierarchy && string.IsNullOrEmpty(ID)) {
                ID = Guid.NewGuid().ToString();
                Debug.Log($"Setting new guid for {nameof(ConversationTrigger)}");
            }
        }
#endif

        [ContextMenu(nameof(LoadBindData))]
        public void LoadBindData() {
            name = DataInGameData.name;
            hasBeenTriggered = DataInGameData.hasBeenTriggered;
        }

        [ContextMenu(nameof(SaveBindData))]
        public void SaveBindData() {
            DataInGameData.name = name;
            DataInGameData.hasBeenTriggered = hasBeenTriggered;
        }
        #endregion
    }
}