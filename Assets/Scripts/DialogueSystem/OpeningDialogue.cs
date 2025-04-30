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
    private ISpeedrunTimer _speedrunTimer;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        _saveLoadSystemManager = container.Resolve<ISaveLoadSystemManager>();
        _levelDataManager = container.Resolve<LevelDataManager>();
        _speedrunTimer = container.Resolve<ISpeedrunTimer>();
    }

    private void Start() {
        Bind(); 
#if UNITY_EDITOR
        if (_levelDataManager.startedInLevel && !forcePlayOnStart) return;
#endif
        if (hasBeenTriggered || (!string.IsNullOrEmpty(_levelDataManager.CurrentCheckpointID) && !forcePlayOnStart)) return;
        
        cutsceneGraph.TriggerUnityEvent("Start");
        hasBeenTriggered = true;
        _speedrunTimer.StartTimer();
    }

    private void OnDestroy() {
        Unbind();
    }
}