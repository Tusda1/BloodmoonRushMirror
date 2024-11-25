using UnityEngine;

public class CameraEffects : MonoBehaviour {
    private void Awake() {
        ServiceLocator.Register(this);
    }

    public async Awaitable ShakeCamera(float intensity, float duration) {
        float currentProgress = 0;

        var originalPosition = transform.position;

        while (currentProgress < 1) {
            await Awaitable.NextFrameAsync();
            currentProgress += Time.deltaTime / duration * Time.timeScale;

            var offsetX = Random.Range(-1f, 1f) * intensity * (1 - currentProgress);
            var offsetY = Random.Range(-1f, 1f) * intensity * (1 - currentProgress);

            var newOffset = new Vector3(offsetX, offsetY, 0);
            transform.position = originalPosition + newOffset;
        }

        transform.position = originalPosition;
    }
}