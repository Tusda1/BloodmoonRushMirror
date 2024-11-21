using System;
using UnityEngine;

public class TeleportTestState : BossState
{
    private Teleport _teleport;
    [SerializeField] private AnimationClip teleportEnter;
    [SerializeField] private AnimationClip teleportExit;

    private void OnEnable()
    {
        _teleport = new Teleport(transform, Boss.AnimancerComponent, teleportEnter, teleportExit);
        
        // Teleport 3 units right
        _teleport.TeleportToLocation(transform.position + Vector3.right * 3);
    }
}
