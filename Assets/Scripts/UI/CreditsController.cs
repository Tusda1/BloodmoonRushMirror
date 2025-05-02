using SceneCollections;
using UnityEngine;
using VContainer;

public class CreditsController : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private SceneCollectionSO sceneCollection;
    
    private CameraEffects _cameraEffects;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        _cameraEffects = container.Resolve<CameraEffects>();
    }

    public void LoadScene() {
        _cameraEffects.FadeLoadScene(sceneCollection);
        PlayerPrefs.SetInt("BeatTheGame", 1);
    }
}