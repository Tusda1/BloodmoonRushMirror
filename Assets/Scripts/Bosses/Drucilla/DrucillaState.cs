using UnityEngine;

public class DrucillaState : BossState 
{
    protected Drucilla DrucillaReference => (Drucilla)Boss;
    
    protected Teleport teleport;
    
    protected virtual void Awake()
    {
        teleport = new Teleport(transform, Boss.AnimancerComponent, OnTeleportEnd);
    }
    
    protected virtual void OnTeleportEnd()
    {
        
    }
}
