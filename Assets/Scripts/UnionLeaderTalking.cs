using System;
using System.Threading;
using TMPro;
using UnityEngine;
using VContainer;

public class UnionLeaderTalking : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private float textSpeed;
    [SerializeField] private string[] lines;
    [SerializeField] private string endLine;
    
    private CancellationTokenSource _cancellationTokenSource;
    
    private IAudioManager _audioManager;
    private CameraEffects _cameraEffects;

    private const string SoundID = "UnionLeaderTalking";
    #endregion

    #region ---Initialization---
    [Inject]
    public void Construct(IObjectResolver container) {
        _audioManager = container.Resolve<IAudioManager>();
        _cameraEffects = container.Resolve<CameraEffects>();
    }
    #endregion
    
    public void StartTaking() {
        _cancellationTokenSource = new CancellationTokenSource();
        _ = StartLineLoop();
    }
    
    public void StopTaking() {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        _ = TypewriterEffect(endLine, () => Invoke(nameof(NewMethod), 0.5f));
    }

    public void TriggerStomping(float seconds, AnimationCurve intensityCurve) {
        _ = Stomp(seconds, intensityCurve);
    }
    
    private async Awaitable Stomp(float seconds, AnimationCurve intensityCurve) {
        var elapsedTime = 0f;
        const float frequency = 1.5f;
    
        while (elapsedTime < seconds) {
            await Awaitable.WaitForSecondsAsync(frequency);
            elapsedTime += frequency;
        
            var intensity = intensityCurve.Evaluate(elapsedTime / seconds);
            _ = _cameraEffects.ShakeCamera(intensity * 0.5f, 0.5f);
            
            // TODO: Set volume of stomping sound
            _audioManager.PlayOneShot("AUB_Stomp");
        }
    }


    private void NewMethod() {
        gameObject.SetActive(false);
    }

    private async Awaitable StartLineLoop() {
        while (!_cancellationTokenSource.IsCancellationRequested) {
            foreach (var line in lines) {
                await TypewriterEffect(line);
                await Awaitable.WaitForSecondsAsync(0.5f);
            }
        }
    }

    private async Awaitable TypewriterEffect(string line, Action onComplete = null) {
        var delay = 1 / textSpeed;
        textField.text = line;

        for (var i = 0; i < line.Length; i++) {
            textField.maxVisibleCharacters = i;
            _audioManager.PlayOneShotAttached(SoundID, gameObject);
            
            await Awaitable.WaitForSecondsAsync(delay, _cancellationTokenSource.Token);
        }
        
        onComplete?.Invoke();
    }
}