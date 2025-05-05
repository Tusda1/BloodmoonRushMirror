using System;
using System.Threading;
using FMOD.Studio;
using TMPro;
using UnityEngine;
using VContainer;

public class UnionLeaderTalking : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private float textSpeed;
    [SerializeField] private string[] lines;
    [SerializeField] private string endLine;
    
    private CancellationTokenSource _cancellationTokenSourceLines;
    private CancellationTokenSource _cancellationTokenSourceEnd;
    private CancellationTokenSource _cancellationTokenSourceStomp;
    
    private EventInstance _eventInstance;
    
    private IAudioManager _audioManager;
    private CameraEffects _cameraEffects;

    #region ***---Constants---***
    private const string UNION_LEADER_TALK_SOUND_ID = "UnionLeaderTalking";
    private const float LINE_PAUSE_DURATION = 2f;
    
    private const string STOMP_SOUND_ID = "AUB_Stomp";
    private const string STOMP_VOLUME_PARAM = "step_volume";
    private const float STOMP_INTERVAL = 1.5f;
    #endregion
    #endregion

    #region ---Initialization---
    [Inject]
    public void Construct(IObjectResolver container) {
        _audioManager = container.Resolve<IAudioManager>();
        _cameraEffects = container.Resolve<CameraEffects>();
    }
    #endregion
    
    public void StartTaking() {
        _cancellationTokenSourceLines = new CancellationTokenSource();
        _ = StartLineLoop();
    }
    
    public void StopTaking() {
        _cancellationTokenSourceLines?.Cancel();
        _cancellationTokenSourceEnd = new CancellationTokenSource();
        _ = TypewriterEffect(endLine, _cancellationTokenSourceEnd, DeactivateGameObject);
    }
    
    public void TriggerStomping(float seconds, AnimationCurve intensityCurve) {
        _cancellationTokenSourceStomp = new CancellationTokenSource();
        _eventInstance = _audioManager.CreateEventInstance(STOMP_SOUND_ID);
        _ = Stomp(seconds, intensityCurve);
    }
    
    private async Awaitable Stomp(float seconds, AnimationCurve intensityCurve) {
        var elapsedTime = 0f;
        
        while (elapsedTime < seconds) {
            await Awaitable.WaitForSecondsAsync(STOMP_INTERVAL, _cancellationTokenSourceStomp.Token);
            elapsedTime += STOMP_INTERVAL;
        
            var intensity = intensityCurve.Evaluate(elapsedTime / seconds);
            _ = _cameraEffects.ShakeCamera(intensity * 0.5f, 0.5f);
            
            _eventInstance.setParameterByName(STOMP_VOLUME_PARAM, intensity);
            _eventInstance.start();
        }
    }

    private void OnDestroy() {
        _cancellationTokenSourceLines?.Cancel();
        _cancellationTokenSourceStomp?.Cancel();
        _cancellationTokenSourceEnd?.Cancel();
    }

    private void DeactivateGameObject() {
        gameObject.SetActive(false);
    }

    private async Awaitable StartLineLoop() {
        while (!_cancellationTokenSourceLines.IsCancellationRequested) {
            foreach (var line in lines) {
                await TypewriterEffect(line, _cancellationTokenSourceLines);
                await Awaitable.WaitForSecondsAsync(LINE_PAUSE_DURATION);
            }
        }
    }

    private async Awaitable TypewriterEffect(string line, CancellationTokenSource tokenSource, Action onComplete = null) {
        var delay = 1 / textSpeed;
        textField.text = line;
        
        _audioManager.PlayOneShotAttached(UNION_LEADER_TALK_SOUND_ID, gameObject);
        
        for (var i = 0; i < line.Length; i++) {
            textField.maxVisibleCharacters = i;
        
            if (line[i] == ' ') {
                _audioManager.PlayOneShotAttached(UNION_LEADER_TALK_SOUND_ID, gameObject);
            }
        
            await Awaitable.WaitForSecondsAsync(delay, tokenSource.Token);
        }
        
        onComplete?.Invoke();
    }
}