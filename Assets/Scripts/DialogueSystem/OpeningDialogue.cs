using Attributes;
using SaveLoadSystem;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class OpeningDialogue : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField] private ScriptMachine cutsceneGraph;
    [SerializeField] private bool forcePlayOnStart;
    
    private LevelDataManager _levelDataManager;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        _levelDataManager = container.Resolve<LevelDataManager>();
    }

    private void Start() {
#if UNITY_EDITOR
        if (_levelDataManager.startedInLevel && !forcePlayOnStart) return;
#endif
        if (string.IsNullOrEmpty(_levelDataManager.CurrentCheckpointID) || forcePlayOnStart) {
            cutsceneGraph.TriggerUnityEvent("Start");
        }
    }
}