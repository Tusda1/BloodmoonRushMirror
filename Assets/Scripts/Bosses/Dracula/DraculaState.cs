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
        teleport = new Teleport(transform, Boss.AnimancerComponent, OnTeleportEnd);
    }
    
    protected virtual void OnTeleportEnd()
    {
        
    }
}
