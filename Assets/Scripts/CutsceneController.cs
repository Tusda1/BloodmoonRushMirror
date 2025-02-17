using System.Collections;
using Attributes;
using SceneCollections;
using UnityEngine;
using UnityEngine.Video;
using VContainer;

public class CutsceneController : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector] private VideoPlayer videoPlayer;
    
    [SerializeField] private SceneCollectionSO firstLevelCollection;
    
    [Header("Cutscene")]
    [SerializeField] private VideoClip newCutscene;
    [SerializeField] private string introSoundID = "Cutscene_Intro";
    
    [Header("Old Cutscene")]
    [SerializeField] private VideoClip oldCutscene;
    [SerializeField] private string oldIntroSoundID = "Cutscene_Intro[Old]";
    
    public bool playOldCutscene;
    
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
        var playOldCutscene = this.playOldCutscene || newCutscene == null;
        
        videoPlayer.clip = playOldCutscene ? oldCutscene : newCutscene;
        videoPlayer.Prepare();

        yield return new WaitUntil(() => videoPlayer.isPrepared);

        videoPlayer.Play();
        _audioManager.StopAllSound();
        _audioManager.PlayOneShot(playOldCutscene ? oldIntroSoundID : introSoundID);
    }

    private void CutsceneEnded(VideoPlayer source) {
        _ = SceneCollectionManager.LoadSceneCollectionAsync(firstLevelCollection);
    }
}