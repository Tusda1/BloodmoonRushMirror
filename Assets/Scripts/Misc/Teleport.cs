using System;
using Animancer;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class Teleport
{
    private readonly Transform _transform;
    private readonly AnimancerComponent _animancerComponent;
    private readonly Action _onTeleportEnd;
    private string _teleportOutPath;
    private string _teleportInPath;
    
    public bool IsTeleporting { get; private set; }
    
    public Teleport(Transform externalTransform, AnimancerComponent externalAnimancerComponent, Action externalOnTeleportEnd = null, string tpOutPath = "", string tpInPath = "")
    {
        _transform = externalTransform;
        _animancerComponent = externalAnimancerComponent;
        _onTeleportEnd = externalOnTeleportEnd;
        _teleportOutPath = tpOutPath;
        _teleportInPath = tpInPath;
    }
    
    public void TeleportToLocation(Vector2 destination, AnimationClip teleportEnter, AnimationClip teleportExit, Action midTeleportAction = null)
    {
        if (Vector2.Distance(_transform.position, destination) < 0.1f)
        {
            _onTeleportEnd.Invoke();
            return;
        }
        
        IsTeleporting = true;
        if (!string.IsNullOrEmpty(_teleportOutPath)) RuntimeManager.PlayOneShotAttached(_teleportOutPath, _transform.gameObject);
        AnimancerState animancerState = _animancerComponent.Play(teleportEnter);
        animancerState.Events(this).OnEnd = () =>
        {
            animancerState.Destroy();
            _transform.position = destination;
            AnimancerState exitState = _animancerComponent.Play(teleportExit);
            if (!string.IsNullOrEmpty(_teleportInPath)) RuntimeManager.PlayOneShotAttached(_teleportInPath, _transform.gameObject);
            midTeleportAction?.Invoke();
            exitState.Time = 0;
            exitState.Events(this).OnEnd = () =>
            {
                exitState.Events(this).OnEnd = null;
                _onTeleportEnd?.Invoke();

                IsTeleporting = false;
                exitState.Destroy();
            };
        };
    }

    public void ResetTeleport()
    {
        IsTeleporting = false;
    }
}
