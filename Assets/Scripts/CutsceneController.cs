using System.Collections;
using Attributes;
using FMODScripts;
using SceneCollections;
using UnityEngine;
using UnityEngine.Video;
using VContainer;

public class CutsceneController : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] private VideoPlayer videoPlayer;
    
    [SerializeField] private VideoClip oldCutscene;
    [SerializeField] private VideoClip newCutscene;
    [SerializeField] private string soundPath = "event:/Sfx/Misc/Cutscene_demo_audio";
    [SerializeField] private SceneCollectionSO firstLevelCollection;
    
    public static bool PlayOldCutscene = false;
    
    [Inject] private CameraEffects _cameraEffects;
    #endregion

    private void Start() {
        StartCoroutine(PlayCutscene());

        videoPlayer.loopPointReached += CutsceneEnded;
        
        _cameraEffects.ClearAllRenderTextures();
    }

    private void OnDestroy() {
        videoPlayer.loopPointReached -= CutsceneEnded;
        StopAllCoroutines();
    }

    private IEnumerator PlayCutscene() {
        videoPlayer.clip = PlayOldCutscene || newCutscene == null ? oldCutscene : newCutscene;
        videoPlayer.Prepare();

        yield return new WaitUntil(() => videoPlayer.isPrepared);

        videoPlayer.Play();
        FMODManager.RaiseStopSoundEvent(StopSoundType.All);
        FMODManager.RaiseEvent(new PlaySoundEvent(soundPath));
    }

    private void CutsceneEnded(VideoPlayer source) {
        _ = SceneCollectionManager.LoadSceneCollectionAsync(firstLevelCollection);
    }
}