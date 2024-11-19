using UnityEngine;

public class InitializationManager : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private SceneCollection startSceneCollection;
    #endregion

    private void Awake() {
        SceneCollectionManager.LoadSceneCollection(startSceneCollection);
    }
}