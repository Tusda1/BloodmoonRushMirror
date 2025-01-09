using Attributes;
using Pathfinding.Util;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Unity.Behavior;
using UnityEngine;
using Utilities.DataBinding;

public class ConversationTrigger : MonoBehaviour, IBind<TriggerData> {
    #region ---Fields---
    #region ***---GetOrAddComponents---***
    [GetOrAddComponent] public BehaviorGraphAgent behaviorGraphAgent;
    #endregion

    private bool HasBeenTriggered {
        get {
            behaviorGraphAgent.BlackboardReference.GetVariableValue<bool>("HasPlayed", out var hasBeenTriggered);
            return hasBeenTriggered;
        }
    }
    
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
        hasBeenTriggered = HasBeenTriggered
    });
    #endregion
    #endregion

    #region ---EventFunctions---
    private void Start() {
        behaviorGraphAgent ??= GetComponent<BehaviorGraphAgent>();
        
        Bind();
    }
    #endregion

    #region ---DataBindingMethods---
    public void Bind() {
        SaveLoadSystemManager.Bind(this, DataInGameData);
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
        behaviorGraphAgent.BlackboardReference.SetVariableValue("HasPlayed", DataInGameData.hasBeenTriggered);
    }

    [ContextMenu(nameof(SaveBindData))]
    public void SaveBindData() {
        DataInGameData.name = name;
        DataInGameData.hasBeenTriggered = HasBeenTriggered;
    }
    #endregion
}