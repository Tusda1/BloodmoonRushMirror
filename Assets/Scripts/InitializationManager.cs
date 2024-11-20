using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class InitializationManager : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private SceneCollectionSO startSceneCollectionSO;
    #endregion

    private void Start() {
        _ = AwaitLoading();
    }

    private async Task AwaitLoading() {
        var scene = SceneManager.GetActiveScene();
        await SceneCollectionManager.LoadSceneCollection(startSceneCollectionSO);
        await SceneManager.UnloadSceneAsync(scene);
    }
}