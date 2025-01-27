using System;
using Animancer;
using UnityEngine;

public class DraculaState : BossState 
{
    private Dracula DraculaReference => (Dracula)Boss;
    
    protected Teleport teleport;

    protected virtual void Awake()
    {
        teleport = new Teleport(transform, Boss.AnimancerComponent, DraculaReference.TeleportEnter, DraculaReference.TeleportExit, OnTeleportEnd);
    }
    
    protected virtual void OnTeleportEnd()
    {
        
    }
}
