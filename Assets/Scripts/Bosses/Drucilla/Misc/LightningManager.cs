using System;
using Animancer;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[Serializable]
public class LightningManager
{
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private AnimationClip lightningAnimation;
    private Pooler _lightningPooler;
    
    public void CreateLightning(Vector2 position, Vector2 targetPosition, Action onLightningEnd = null)
    {
        _lightningPooler ??= TryGetCommunityPooler.TryGetPooler(lightningPrefab);
        
        var exampleLightning = _lightningPooler.GetPooledObject();
        exampleLightning.SetActive(true);
        var length = exampleLightning.GetComponent<Collider2D>().bounds.size.y;
        exampleLightning.SetActive(false);
        
        // Determine how many lightning bolts are needed to reach the target
        var distance = Vector2.Distance(position, targetPosition);
        var direction = (targetPosition - position).normalized;
        var lightningCount = Mathf.CeilToInt(distance / length);

        for (int i = 0; i < lightningCount; i++)
        {
            var lightning = _lightningPooler.GetPooledObject();
            lightning.transform.position = position + direction * (length * i);
            lightning.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.down, direction));
            lightning.SetActive(true);

            var p = i;
            var lightningAnimancer = lightning.GetComponent<AnimancerComponent>().Play(lightningAnimation);
            lightningAnimancer.Events(this).OnEnd ??= () =>
            {
                if (lightningCount - 1 == p) onLightningEnd?.Invoke();
                lightningAnimancer.Events(this).OnEnd = null;
                lightning.SetActive(false);
            };
        }
    }
}
