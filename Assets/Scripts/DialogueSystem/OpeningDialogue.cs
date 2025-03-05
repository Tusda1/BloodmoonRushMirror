using Attributes;
using SaveLoadSystem;
using SceneCollections;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class OpeningDialogue : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] private ScriptMachine cutsceneGraph;
    [SerializeField] private bool forcePlayOnStart;
    
    [Inject] private LevelDataManager _levelDataManager;
    #endregion

    private void Start() {
#if UNITY_EDITOR
        if (SceneCollectionManager.CurrentSceneCollection == null && !forcePlayOnStart) return;
#endif
        if (string.IsNullOrEmpty(_levelDataManager.CurrentCheckpointData.ID) || forcePlayOnStart) {
            cutsceneGraph.TriggerUnityEvent("Start");
        }
    }
}