using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializationManager : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private SceneCollection startSceneCollection;
    #endregion

    private void Start() {
        _ = AwaitLoading();
    }

    private async Task AwaitLoading() {
        var scene = SceneManager.GetActiveScene();
        await SceneCollectionManager.LoadSceneCollection(startSceneCollection);
        SceneManager.UnloadSceneAsync(scene);
    }
}