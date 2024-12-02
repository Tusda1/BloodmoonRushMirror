using System;
using UnityEngine;

[Serializable]
public class SmoothDampScript
{
    [SerializeField] private float smoothTime = 0.1f;
    
    private Vector2 currentVelocity;
    private Vector2 smoothVelocity;
    
    public void SetStartVelocity(Vector2 startVelocity)
    {
        smoothVelocity = startVelocity;
    }
    
    public void SmoothDampFloat(Vector2 facingDirection, float moveSpeed, ref Vector2 targetVelocity)
    {
        smoothVelocity = Vector2.SmoothDamp(smoothVelocity, facingDirection * moveSpeed, ref currentVelocity, smoothTime);
        targetVelocity = smoothVelocity;
    }
    
    public void SmoothDampVector2(Vector2 target, ref Vector2 targetVelocity)
    {
        smoothVelocity = Vector2.SmoothDamp(smoothVelocity, target, ref currentVelocity, smoothTime);
        targetVelocity = smoothVelocity;
    }
}
