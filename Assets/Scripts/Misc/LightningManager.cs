using System;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[Serializable]
public class LightningManager
{
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private AnimationClip lightningAnimation;
    private Pooler _lightningPooler;
    
    public void CreateLightning(Vector2 position, Vector2 targetPosition)
    {
        _lightningPooler ??= TryGetCommunityPooler.TryGetPooler(lightningPrefab);
        
        var length = lightningPrefab.GetComponent<Collider2D>().bounds.size.y;
        
        // Determine how many lightning bolts are needed to reach the target
        var distance = Vector2.Distance(position, targetPosition);
        var lightningCount = Mathf.CeilToInt(distance / length);

        for (int i = 0; i < lightningCount; i++)
        {
            var lightning = _lightningPooler.GetPooledObject();
            lightning.transform.position = Vector2.Lerp(position, targetPosition, i / (float) lightningCount);
            lightning.SetActive(true);
        }
    }
}
