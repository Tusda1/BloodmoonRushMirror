using System;
using SaveLoadSystem.Data;
using UnityEngine;

public partial class OpeningDialogue {
    #region ---DataBindingVariables---
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

    #region ---DataBindingMethods---
    public void Bind() {
        _saveLoadSystemManager.DataBinder.Bind(this, DataInGameData);
    }

    public void Unbind() {
        _saveLoadSystemManager.DataBinder.UnBind(this, DataInGameData);
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