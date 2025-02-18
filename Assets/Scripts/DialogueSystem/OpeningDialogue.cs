using System.Collections;
using Attributes;
using Consensus;
using SaveLoadSystem;
using SceneCollections;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class OpeningDialogue : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] private ScriptMachine cutsceneGraph;
    [SerializeField] private bool forcePlayOnStart;

    [HideInInspector] public ScriptVotePair scriptVotePair = new();
    
    private LevelDataManager _levelDataManager;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        _levelDataManager = container.Resolve<LevelDataManager>();
    }

    private IEnumerator Start() {
        yield return null;
        
#if UNITY_EDITOR
        if (SceneCollectionManager.CurrentSceneCollection == null && !forcePlayOnStart) yield break;
#endif
        if (string.IsNullOrEmpty(_levelDataManager.CurrentCheckpointData.ID) || forcePlayOnStart) {
            cutsceneGraph.TriggerUnityEvent("Start");
        }
    }
}