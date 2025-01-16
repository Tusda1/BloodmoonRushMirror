using UnityEngine;

public class DrucillaState : BossState 
{
    protected Drucilla DrucillaReference => (Drucilla)Boss;
    
    protected Teleport teleport;
    
    private void Awake()
    {
        teleport = new Teleport(transform, Boss.AnimancerComponent, DrucillaReference.TeleportEnter, DrucillaReference.TeleportExit, OnTeleportEnd);
    }
    
    protected virtual void OnTeleportEnd()
    {
        
    }
}
