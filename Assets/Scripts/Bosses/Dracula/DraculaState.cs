using System;
using Animancer;
using UnityEngine;

public class DraculaState : BossState 
{
    [SerializeField] protected AnimationClip teleportEnterAir;
    [SerializeField] protected AnimationClip teleportEnterGround;
    [SerializeField] protected AnimationClip teleportExit;
    [SerializeField] protected AnimationClip idleAnimation;
    
    protected Teleport teleport;

    protected virtual void Awake()
    {
        teleport = new Teleport(transform, Boss.AnimancerComponent, OnTeleportEnd, "event:/Sfx/Boss/Dracula/Drac_tp_out", "event:/Sfx/Boss/Dracula/Drac_tp_in");
    }
    
    protected virtual void OnTeleportEnd()
    {
        
    }

    protected virtual void OnDisable()
    {
        if (teleport != null)
        {
            teleport.ResetTeleport();
        }
    }
}
