using System;
using UnityEngine;

public class MachineGunState : DrucillaState
{
    [SerializeField] private GameObject projectilePrefab;
    
    private Pooler _pooler;
    
    private enum MachineGunStage
    {
        Teleport,
        Shoot
    }
    
    private MachineGunStage _stage;

    private void OnEnable()
    {
        
    }

    private void Update()
    {
        switch (_stage)
        {
            case MachineGunStage.Teleport:
                if (!teleport.IsTeleporting) print("hji");
                break;
            case MachineGunStage.Shoot:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
