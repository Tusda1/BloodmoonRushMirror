using System.Collections;
using Attributes;
using SceneCollections;
using UnityEngine;
using UnityEngine.Video;
using VContainer;

public class CutsceneController : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] private VideoPlayer videoPlayer;
    
    [SerializeField] private SceneCollectionSO loadCollectionAfterCutscene;
    
    [Header("Cutscene")]
    [SerializeField] private VideoClip cutscene;
    [SerializeField] private string introSoundID = "Cutscene_Intro";
    
    [Inject] private CameraEffects _cameraEffects;
    [Inject] private IAudioManager _audioManager;
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
        videoPlayer.clip = cutscene;
        videoPlayer.Prepare();

        yield return new WaitUntil(() => videoPlayer.isPrepared);
        
        videoPlayer.Play();
        _audioManager.StopAllSound();
        _audioManager.PlayOneShot(introSoundID);
    }

    private void CutsceneEnded(VideoPlayer source) {
        _ = SceneCollectionManager.LoadSceneCollectionAsync(loadCollectionAfterCutscene);
    }
}