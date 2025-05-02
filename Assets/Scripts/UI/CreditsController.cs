using SceneCollections;
using UnityEngine;
using VContainer;

public class CreditsController : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private SceneCollectionSO sceneCollection;
    [SerializeField] private string musicID;
    
    private CameraEffects _cameraEffects;
    private IAudioManager _audioManager;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        _cameraEffects = container.Resolve<CameraEffects>();
        _audioManager = container.Resolve<IAudioManager>();
    }

    public void LoadScene() {
        _cameraEffects.FadeLoadScene(sceneCollection);
        _audioManager.PlayMusic(musicID);
        PlayerPrefs.SetInt("BeatTheGame", 1);
    }
}