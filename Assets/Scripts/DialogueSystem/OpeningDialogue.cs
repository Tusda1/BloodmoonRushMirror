using System.Collections;
using Attributes;
using SaveLoadSystem;
using SceneCollections;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

public class OpeningDialogue : MonoBehaviour {
    #region ---Fields---
    [SerializeField, Required] private ActionEventChannel openingInteractionChannel;
    [SerializeField, Required] private GameObject moveInputAnimation;
    [SerializeField] private bool forcePlayOnStart;
    
    [SerializeField] private ScriptMachine cutsceneGraph;
    
    [HideInInspector] public LevelDataManager levelDataManager;
    [HideInInspector] public LevelMusicManager levelMusicManager;
    [HideInInspector] public CameraEffects cameraEffects;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        levelDataManager = container.Resolve<LevelDataManager>();
        levelMusicManager = container.Resolve<LevelMusicManager>();
        cameraEffects = container.Resolve<CameraEffects>();
    }

    private IEnumerator Start() {
        yield return null;
        
#if UNITY_EDITOR
        if (SceneCollectionManager.CurrentSceneCollection == null && !forcePlayOnStart) yield break;
#endif
        if (string.IsNullOrEmpty(levelDataManager.CurrentCheckpointData.ID) || forcePlayOnStart) {
            openingInteractionChannel.SendEventMessage();

            // cutsceneGraph.TriggerUnityEvent("Start");
        }
        else {
            moveInputAnimation.SetActive(true);
        }
    }
}