using System;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
using SceneCollections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;
using Random = UnityEngine.Random;

public class CameraEffects : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private AnimationCurve timeSlowCurve;
    [SerializeField] private GameObject cameraPixelatedGameObject;
    [SerializeField] private Volume cameraVolume;
    
    [Header("CircleFade")]
    [SerializeField] private Material circleFadeMaterial;
    [SerializeField] private AnimationCurve circleFadeInCurve;
    [SerializeField] private AnimationCurve circleFadeOutCurve;

    [Header("Render Textures")] 
    [SerializeField] private RenderTexture cameraPixelated;
    [SerializeField] private RenderTexture cameraNonPixelated;
    [SerializeField] private RenderTexture uiPixelated;
    [SerializeField] private RenderTexture uiNonPixelated;

    private const float FadeCircleDefaultSize = 2.5f;
    
    private CancellationTokenSource _cancellationTokenSource = new();
    private CancellationTokenSource _timeSlow = new();
    
    private IPauseManager _pauseManager;

    private ChromaticAberration _chromaticAberration;

    #region ***---ShaderPropertyID---***
    private static readonly int Size = Shader.PropertyToID("_Size");
    #endregion
    #endregion

    #region ---Initialization---
    [Inject]
    public void Construct(IObjectResolver container) {
        _pauseManager = container.Resolve<IPauseManager>();
    }

    private void Start() {
        cameraVolume.profile.TryGet(out _chromaticAberration);
        _chromaticAberration.intensity.value = 0;
        
        _pauseManager.OnPause += () => {
            _timeSlow.Cancel();
            _timeSlow = new CancellationTokenSource();
        };
    }
    #endregion

    private void OnEnable() {
        circleFadeMaterial.SetFloat(Size, FadeCircleDefaultSize);
        ClearAllRenderTextures();
    }

    private void OnDisable() {
        circleFadeMaterial.SetFloat(Size, FadeCircleDefaultSize);
    }

    public void ClearAllRenderTextures() {
        cameraPixelated.Release();
        cameraNonPixelated.Release();
        uiPixelated.Release();
        uiNonPixelated.Release();
    }

    public async Awaitable ShakeCamera(float intensity, float duration) {
        float currentProgress = 0;

        var originalPosition = transform.position;

        while (currentProgress < duration) {
            currentProgress += Time.unscaledDeltaTime;

            var offsetX = Random.Range(-1f, 1f) * intensity * (1 - currentProgress);
            var offsetY = Random.Range(-1f, 1f) * intensity * (1 - currentProgress);

            var newOffset = new Vector3(offsetX, offsetY, 0);
            transform.position = originalPosition + newOffset;
            
            await Awaitable.NextFrameAsync(_cancellationTokenSource.Token);
        }

        transform.position = originalPosition;
    }

    public async Awaitable TimeSlow(float duration, AnimationCurve curve = null) {
        curve ??= timeSlowCurve;
        
        float currentProgress = 0;

        while (currentProgress < duration) {
            currentProgress += Time.unscaledDeltaTime;
            
            Time.timeScale = Mathf.Clamp01(curve.Evaluate(currentProgress));

            await Awaitable.NextFrameAsync(_timeSlow.Token);
        }

        Time.timeScale = 1;
    }

    public async Awaitable CircleFade(float duration, AnimationCurve curve, [Optional] Action callback) {
        float currentProgress = 0;

        while (currentProgress < duration) {
            currentProgress += Time.unscaledDeltaTime;
            
            var progressValue = curve.Evaluate(currentProgress / duration) * FadeCircleDefaultSize;
            circleFadeMaterial.SetFloat(Size, progressValue);

            await Awaitable.NextFrameAsync();
        }
        
        callback?.Invoke();
    }
    
    public async Awaitable MoveCamera(float duration, AnimationCurve curve, Vector3 offset, [Optional] Action callback) {
        float currentProgress = 0;

        while (currentProgress < duration) {
            currentProgress += Time.unscaledDeltaTime;
            
            var progressValue = curve.Evaluate(currentProgress / duration);
            cameraPixelatedGameObject.transform.position = Vector3.Lerp(cameraPixelatedGameObject.transform.position, offset, progressValue);

            await Awaitable.NextFrameAsync(_cancellationTokenSource.Token);
        }
        
        cameraPixelatedGameObject.transform.position = offset;
        
        callback?.Invoke();
    }

    public void CircleFadeIn(float duration, [Optional] Action callback) {
        _ = CircleFade(duration, circleFadeInCurve, callback);
    }
    
    public void CircleFadeOut(float duration, [Optional] Action callback) {
        _ = CircleFade(duration, circleFadeOutCurve, callback);
    }

    public void TriggerMoveCamera(float duration, Vector3 offset, [Optional] Action callback) {
        var curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        _ = MoveCamera(duration, curve, offset, callback);
    }

    public void ResetCameraMove() {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        
        cameraPixelatedGameObject.transform.position = Vector3.zero;
    }

    public void FadeLoadScene(SceneCollectionSO sceneCollection, [CanBeNull]Action callback = null) {
        CircleFadeOut(1,
            () => {
                Time.timeScale = 0;
                _ = SceneCollectionManager.LoadSceneCollectionAsync(sceneCollection,
                    () => {
                        CircleFadeIn(1);
                        Time.timeScale = 1;
                        callback?.Invoke();
                    });
            });
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(TestMoveCamera))]
    private void TestMoveCamera() {
        TriggerMoveCamera(2, Vector3.down * 2);
    }
    
    [ContextMenu(nameof(TestShakeCamera))]
    private void TestShakeCamera() {
        _ = ShakeCamera(0.2f, 0.2f);
    }

    [ContextMenu(nameof(TestTimeSlow))]
    private void TestTimeSlow() {
        _ = TimeSlow(0.5f);
    }
#endif
}