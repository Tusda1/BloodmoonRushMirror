using SceneCollections;
using UnityEngine;
using VContainer;

public class ExitBoilerLadder : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private SceneCollectionSO nextLevelSceneCollection;
    private CameraEffects _cameraEffects;
    #endregion
    
    [Inject]
    public void Construct(IObjectResolver container) {
        _cameraEffects = container.Resolve<CameraEffects>();
    }
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            _cameraEffects.FadeLoadScene(nextLevelSceneCollection);
        }
    }
}