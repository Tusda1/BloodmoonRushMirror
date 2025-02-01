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
        
        var lightning = _lightningPooler.GetPooledObject();
        var length = lightning.GetComponent<Collider2D>().bounds.size.x;
    }
}
