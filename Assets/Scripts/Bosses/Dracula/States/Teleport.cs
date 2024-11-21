using Animancer;
using UnityEngine;

public class Teleport
{
    private Transform transform;
    private AnimancerComponent animancerComponent;
    private AnimationClip teleportEnter;
    private AnimationClip teleportExit;
    
    public Teleport(Transform externalTransform, AnimancerComponent externalAnimancerComponent, AnimationClip externalTeleportEnter, AnimationClip externalTeleportExit)
    {
        transform = externalTransform;
        animancerComponent = externalAnimancerComponent;
        teleportEnter = externalTeleportEnter;
        teleportExit = externalTeleportExit;
    }
    
    public void TeleportToLocation(Vector2 destination)
    {
        AnimancerState animancerState = animancerComponent.Play(teleportEnter);
        animancerState.Events(this).OnEnd ??= () =>
        {
            Debug.Log("starting exit animation");
            transform.position = destination;
            animancerComponent.Play(teleportExit).Time = 0;
            animancerState.Events(this).OnEnd = null;
        };
    }
}
