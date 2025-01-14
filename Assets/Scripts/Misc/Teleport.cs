using System;
using Animancer;
using UnityEngine;

public class Teleport
{
    private readonly Transform _transform;
    private readonly AnimancerComponent _animancerComponent;
    private readonly AnimationClip _teleportEnter;
    private readonly AnimationClip _teleportExit;
    private readonly Action _onTeleportEnd;
    
    public bool IsTeleporting { get; private set; }
    
    public Teleport(Transform externalTransform, AnimancerComponent externalAnimancerComponent, AnimationClip externalTeleportEnter, AnimationClip externalTeleportExit, Action externalOnTeleportEnd = null)
    {
        _transform = externalTransform;
        _animancerComponent = externalAnimancerComponent;
        _teleportEnter = externalTeleportEnter;
        _teleportExit = externalTeleportExit;
        _onTeleportEnd = externalOnTeleportEnd;
    }
    
    public void TeleportToLocation(Vector2 destination)
    {
        if (Vector2.Distance(_transform.position, destination) < 0.1f)
        {
            _onTeleportEnd.Invoke();
            Debug.Log("Teleport destination same as current position. Teleport cancelled.");
            return;
        }
        
        IsTeleporting = true;
        
        AnimancerState animancerState = _animancerComponent.Play(_teleportEnter);
        animancerState.Events(this).OnEnd ??= () =>
        {
            _transform.position = destination;
            _animancerComponent.Play(_teleportExit).Time = 0;
            animancerState.Events(this).OnEnd = () =>
            {
                animancerState.Stop();
                animancerState.Events(this).OnEnd = null;
                animancerState.Destroy();
                _onTeleportEnd?.Invoke();
                
                IsTeleporting = false;
            };
        };
    }
}
