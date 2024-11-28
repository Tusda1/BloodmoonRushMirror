using System.Threading.Tasks;
using Attributes;
using UnityEngine;

public class InitializationManager : MonoBehaviour {
    #region ---Fields---
    [SerializeField, Required] private SceneCollectionSO thisSceneCollectionSO;
    [SerializeField, Required] private SceneCollectionSO startSceneCollectionSO;
    #endregion

    private void Start() {
        _ = AwaitLoading();
    }

    private async Task AwaitLoading() {
        foreach (var sceneInstance in thisSceneCollectionSO.scenes) {
            SceneCollectionManager.AddToActiveScenes(sceneInstance);
        }
        
        await SceneCollectionManager.LoadSceneCollectionAsync(startSceneCollectionSO);
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(DebugSceneCollection))]
    public void DebugSceneCollection() {
        Debug.Log(SceneCollectionManager.CurrentSceneCollectionSO != null);
    }
#endif
}