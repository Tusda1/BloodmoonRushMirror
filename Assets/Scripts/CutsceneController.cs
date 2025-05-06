using System.Collections;
using Attributes;
using SceneCollections;
using UnityEngine;
using UnityEngine.Video;
using VContainer;
#if STEAM_BUILD
using Steamworks;
#endif

public class CutsceneController : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] private VideoPlayer videoPlayer;
    
    [SerializeField] private SceneCollectionSO loadCollectionAfterCutscene;
    
    [Header("Cutscene")]
    [SerializeField] private VideoClip cutscene;
    [SerializeField] private string introSoundID = "Cutscene_Intro";
    [SerializeField] private string steamStatus = "#status_cutscene_";
    
    private CameraEffects _cameraEffects;
    private IAudioManager _audioManager;
    #endregion

    [Inject]
    public void Construct(IObjectResolver container) {
        _cameraEffects = container.Resolve<CameraEffects>();
        _audioManager = container.Resolve<IAudioManager>();
    }

    private void Start() {
        StartCoroutine(PlayCutscene());
#if STEAM_BUILD
            SteamFriends.SetRichPresence("status", steamStatus);
#endif
        videoPlayer.loopPointReached += CutsceneEnded;
        
        _cameraEffects.ClearAllRenderTextures();
    }

    private void OnDestroy() {
        videoPlayer.loopPointReached -= CutsceneEnded;
        StopAllCoroutines();
    }

    private IEnumerator PlayCutscene() {
        videoPlayer.clip = cutscene;
        videoPlayer.Prepare();

        yield return new WaitUntil(() => videoPlayer.isPrepared);
        
        videoPlayer.Play();
        _audioManager.StopAllSound();
        _audioManager.PlayOneShot(introSoundID);
    }

    private void CutsceneEnded(VideoPlayer source) {
        _cameraEffects.FadeLoadScene(loadCollectionAfterCutscene);
    }
}