using System;
using UnityEngine;

public class TeleportTestState : DraculaState
{
    private Teleport _teleport;
    [SerializeField] private AnimationClip teleportEnter;
    [SerializeField] private AnimationClip teleportExit;

    private void OnEnable()
    {
        _teleport = new Teleport(transform, Boss.AnimancerComponent, teleportEnter, teleportExit, OnTeleportEnd);
        
        // Teleport 3 units right
        _teleport.TeleportToLocation(transform.position + Vector3.right * 3);
    }
    
    private void OnTeleportEnd()
    {
        print("Teleport ended");
    }
}
