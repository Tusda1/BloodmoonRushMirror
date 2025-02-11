using System;
using Animancer;
using UnityEngine;

public class Teleport
{
    private readonly Transform _transform;
    private readonly AnimancerComponent _animancerComponent;
    private readonly Action _onTeleportEnd;
    
    public bool IsTeleporting { get; private set; }
    
    public Teleport(Transform externalTransform, AnimancerComponent externalAnimancerComponent, Action externalOnTeleportEnd = null)
    {
        _transform = externalTransform;
        _animancerComponent = externalAnimancerComponent;
        _onTeleportEnd = externalOnTeleportEnd;
    }
    
    public void TeleportToLocation(Vector2 destination, AnimationClip teleportEnter, AnimationClip teleportExit)
    {
        if (Vector2.Distance(_transform.position, destination) < 0.1f)
        {
            _onTeleportEnd.Invoke();
            Debug.Log("Teleport destination same as current position. Teleport cancelled.");
            return;
        }
        
        IsTeleporting = true;
        
        AnimancerState animancerState = _animancerComponent.Play(teleportEnter);
        animancerState.Events(this).OnEnd ??= () =>
        {
            _transform.position = destination;
            AnimancerState exitState = _animancerComponent.Play(teleportExit);
            exitState.Time = 0;
            exitState.Events(this).OnEnd = () =>
            {
                exitState.Events(this).OnEnd = null;
                _onTeleportEnd?.Invoke();

                IsTeleporting = false;
                exitState.Destroy();
                animancerState.Destroy();
            };
        };
    }
}
