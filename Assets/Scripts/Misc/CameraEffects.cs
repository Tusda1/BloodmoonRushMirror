using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraEffects : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private AnimationCurve timeSlowCurve;
    
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

    #region ***---ShaderPropertyID---***
    private static readonly int Size = Shader.PropertyToID("_Size");
    #endregion
    #endregion

    private void OnEnable() {
        circleFadeMaterial.SetFloat(Size, FadeCircleDefaultSize);
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

            await Awaitable.NextFrameAsync();
        }

        transform.position = originalPosition;
    }

    public async Awaitable TimeSlow(float duration, AnimationCurve curve = null) {
        curve ??= timeSlowCurve;
        
        float currentProgress = 0;

        while (currentProgress < duration) {
            currentProgress += Time.unscaledDeltaTime;
            
            Time.timeScale = Mathf.Clamp01(curve.Evaluate(currentProgress));

            await Awaitable.NextFrameAsync();
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

    [ContextMenu(nameof(CircleFadeIn))]
    public void CircleFadeIn(float duration, [Optional] Action callback) {
        _ = CircleFade(duration, circleFadeInCurve, callback);
    }

    [ContextMenu(nameof(CircleFadeOut))]
    public void CircleFadeOut(float duration, [Optional] Action callback) {
        _ = CircleFade(duration, circleFadeOutCurve, callback);
    }

#if UNITY_EDITOR
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