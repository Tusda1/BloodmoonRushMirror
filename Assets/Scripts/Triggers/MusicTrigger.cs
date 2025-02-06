using SaveLoadSystem;
using SaveLoadSystem.Data;
using UnityEngine;
using Utilities.DataBinding;
using VContainer;
#if UNITY_EDITOR
using System;
#endif

namespace Triggers {
    public class MusicTrigger : TriggerBox, IBind<TriggerData> {
        #region ---Fields---
        [SerializeField] private string musicID;
        [SerializeField] private int musicStage;
    
        #region ***---GameManagers---***
        private static SaveLoadSystemManager SaveLoadSystemManager => SaveLoadSystemManager.Instance;
        private static LevelDataManager LevelDataManager => LevelDataManager.LevelDataManagerCache;
        #endregion

        #region ***---DataBindingVariables---***
        [field: Header("Data Binding")]
        [field: SerializeField] public string ID { get; set; }

        [field: SerializeField] public TriggerData SaveSystemData { get; set; }

        public TriggerData DataInGameData => LevelDataManager.DataInGameData.triggerDatas.GetOrAddByIdInList(ID, new TriggerData {
            name = name,
            ID = ID,
            hasBeenTriggered = hasBeenTriggered
        });
        #endregion
        
        [Inject] public IAudioManager AudioManager;
        #endregion
        
        #region ---EventFunctions---
        private void Start() {
            Bind();
        }
        #endregion

        protected override void TriggeredAction(Collider2D other) {
            AudioManager.PlayMusic(musicID);
            AudioManager.SetParameterValue(IAudioManager.LevelProgressionParameter, musicStage);
        }
        
        #region ---DataBindingMethods---
        public void Bind() {
            if (SaveLoadSystemManager != null || !Application.isPlaying) {
                SaveLoadSystemManager.Bind(this, DataInGameData);
            }
            else {
                Debug.LogWarning("No SaveLoadSystemManager found", this);
            }
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