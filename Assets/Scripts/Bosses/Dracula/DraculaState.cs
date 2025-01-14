using System;
using Animancer;
using UnityEngine;

public enum DraculaStateCategory
{
    Phase1,
    Phase2,
    General
}

public class DraculaState : BossState 
{
    [SerializeField] private DraculaStateCategory category;
    public DraculaStateCategory Category => category;
    
    protected Dracula DraculaReference => (Dracula)Boss;
    
    protected Teleport teleport;

    private void Awake()
    {
        teleport = new Teleport(transform, Boss.AnimancerComponent, DraculaReference.TeleportEnter, DraculaReference.TeleportExit, OnTeleportEnd);
    }
    
    protected virtual void OnTeleportEnd()
    {
        
    }
}
