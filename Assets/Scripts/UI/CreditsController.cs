using System;
using SceneCollections;
using UnityEngine;
using VContainer;
#if STEAM_BUILD
using Steamworks;
#endif

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

    private void Start() {
        _audioManager.PlayMusic(musicID);
#if STEAM_BUILD
            SteamFriends.SetRichPresence("status", "#status_credits");
#endif
    }

    public void LoadScene() {
        _cameraEffects.FadeLoadScene(sceneCollection);
        PlayerPrefs.SetInt("BeatTheGame", 1);
    }
}