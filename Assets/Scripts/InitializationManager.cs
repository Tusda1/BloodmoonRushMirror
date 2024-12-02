using Attributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.Assertions;
#endif

public class InitializationManager : MonoBehaviour {
    #region ---Fields---
    [SerializeField, Required] private SceneCollectionSO thisSceneCollection;
    [SerializeField, Required] private SceneCollectionSO startSceneCollection;
    #endregion

    private void Start() {
#if UNITY_EDITOR
        Assert.IsNotNull(thisSceneCollection);

        if (SceneCollectionManager.CurrentSceneCollectionSO == null ||
            SceneCollectionManager.CurrentSceneCollectionSO != thisSceneCollection) {
            SceneCollectionManager.CurrentSceneCollectionSO = thisSceneCollection;
        }
#endif

        SceneCollectionManager.ClearActiveScenes();

        foreach (var sceneInstance in thisSceneCollection.scenes) {
            SceneCollectionManager.AddToActiveScenes(sceneInstance);
        }

        Debug.Log("Loading initial scene collection...");

        _ = SceneCollectionManager.LoadSceneCollectionAsync(startSceneCollection);
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(DebugSceneCollection))]
    public void DebugSceneCollection() {
        Debug.Log(SceneCollectionManager.CurrentSceneCollectionSO != null);
    }
#endif
}