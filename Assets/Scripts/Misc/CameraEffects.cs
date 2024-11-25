using System.Collections;
using UnityEngine;

public class CameraEffects : MonoBehaviour {
    #region MyRegion
    [SerializeField] private AnimationCurve timeSlowCurve;
    #endregion
    
    private void Awake() {
        ServiceLocator.Register(this);
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

    public IEnumerator TimeSlow(float duration) {
        float currentProgress = 0;

        while (currentProgress < duration) {
            currentProgress += Time.unscaledDeltaTime;
            
            Time.timeScale = Mathf.Clamp01(timeSlowCurve.Evaluate(currentProgress));

            yield return null;
            // await Awaitable.NextFrameAsync();
        }
        
        Time.timeScale = 1;
    }
}