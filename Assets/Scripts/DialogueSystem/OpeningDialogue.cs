using Attributes;
using SaveLoadSystem;
using SaveLoadSystem.Data;
using Unity.VisualScripting;
using UnityEngine;
using Utilities.DataBinding;
using VContainer;

public partial class OpeningDialogue : MonoBehaviour, IBind<TriggerData> {
    #region ---Fields---
    [GetOrAddComponent, SerializeField] private ScriptMachine cutsceneGraph;
    [SerializeField] private bool forcePlayOnStart;
    [SerializeField] private bool hasBeenTriggered;

    private ISaveLoadSystemManager _saveLoadSystemManager;
    private LevelDataManager _levelDataManager;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
        _levelDataManager = container.Resolve<LevelDataManager>();
    }

    private void Start() {
        Bind();
#if UNITY_EDITOR
        if (_levelDataManager.startedInLevel && !forcePlayOnStart) return;
#endif
        if (hasBeenTriggered || (!string.IsNullOrEmpty(_levelDataManager.CurrentCheckpointID) && !forcePlayOnStart)) return;
        
        cutsceneGraph.TriggerUnityEvent("Start");
        hasBeenTriggered = true;
    }

    private void OnDestroy() {
        Unbind();
    }
}