using System;
using Attributes;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Triggers;
using Unity.VisualScripting;
using UnityEngine;
using Utilities.DataBinding;
using VContainer;
using Guid = Pathfinding.Util.Guid;

public class ConversationTrigger : TriggerBox, IBind<TriggerData> {
    #region ---Fields---
    #region ***---GetOrAddComponents---***
    [GetOrAddComponent, SerializeField, HideInInspector] private ScriptMachine cutsceneGraph;
    #endregion

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
    private LevelDataManager _levelDataManager;
    #endregion

    #region ---Initialization---
    [Inject]
    public void Construct(IObjectResolver container) {
#if SAVESYSTEM_ACTIVE
        _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
#endif
        _levelDataManager = container.Resolve<LevelDataManager>();
    }
    
    private void Start() {
        Bind();
    }
    #endregion

    protected override void TriggeredAction(Collider2D other) {
        cutsceneGraph.TriggerUnityEvent("Start");
    }

    #region ---DataBindingMethods---
    public void Bind() {
#if SAVESYSTEM_ACTIVE
        _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
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