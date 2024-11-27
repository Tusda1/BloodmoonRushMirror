using System;
using Animancer;
using UnityEngine;

public class Teleport
{
    private readonly Transform transform;
    private readonly AnimancerComponent animancerComponent;
    private readonly AnimationClip teleportEnter;
    private readonly AnimationClip teleportExit;
    private readonly Action onTeleportEnd;
    
    public bool IsTeleporting { get; private set; }
    
    public Teleport(Transform externalTransform, AnimancerComponent externalAnimancerComponent, AnimationClip externalTeleportEnter, AnimationClip externalTeleportExit, Action externalOnTeleportEnd = null)
    {
        transform = externalTransform;
        animancerComponent = externalAnimancerComponent;
        teleportEnter = externalTeleportEnter;
        teleportExit = externalTeleportExit;
        onTeleportEnd = externalOnTeleportEnd;
    }
    
    public void TeleportToLocation(Vector2 destination)
    {
        IsTeleporting = true;
        
        AnimancerState animancerState = animancerComponent.Play(teleportEnter);
        animancerState.Events(this).OnEnd ??= () =>
        {
            transform.position = destination;
            animancerComponent.Play(teleportExit).Time = 0;
            animancerState.Events(this).OnEnd = () =>
            {
                animancerState.Stop();
                onTeleportEnd?.Invoke();
                
                IsTeleporting = false;
            };
        };
    }
}
