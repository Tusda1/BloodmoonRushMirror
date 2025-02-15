using System.Collections;
using Attributes;
using SaveLoadSystem;
using SceneCollections;
using UnityEngine;
using VContainer;

public class OpeningDialogue : MonoBehaviour {
    #region ---Fields---
    [SerializeField, Required] private ActionEventChannel openingInteractionChannel;
    [SerializeField, Required] private GameObject moveInputAnimation;
    [SerializeField] private bool forcePlayOnStart;
    
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
            openingInteractionChannel.SendEventMessage();
        }
        else {
            moveInputAnimation.SetActive(true);
        }
    }
}