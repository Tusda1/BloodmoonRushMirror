using Attributes;
using SaveLoadSystem;
using SceneCollections;
using UnityEngine;
using VContainer;

public class OpeningDialogue : MonoBehaviour {
    #region ---Fields---
    [SerializeField, Required] private ActionEventChannel openingInteractionChannel;
    
    private LevelDataManager _levelDataManager;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        _levelDataManager = container.Resolve<LevelDataManager>();
    }

    private void Start() {
#if UNITY_EDITOR
        if (SceneCollectionManager.CurrentSceneCollection == null) return;
#endif
        if (string.IsNullOrEmpty(_levelDataManager.CurrentCheckpointData.ID)) {
            openingInteractionChannel.SendEventMessage();
        }
    }
}