using System;
using UnityEngine;

public class BombAttackState : DrucillaState
{
    [SerializeField] private GameObject bombPrefab;
    
    private enum BombAttackStage
    {
        Teleport,
        DropBombs,
        GroundSlam
    }
    
    private BombAttackStage _stage;

    private void Start()
    {
        
    }

    private void OnEnable()
    {
        
    }

    private void Update()
    {
        switch (_stage)
        {
            case BombAttackStage.Teleport:
                
                break;
        }
    }

    private void DropBombs()
    {
        
    }
    
    private void GroundSlam()
    {
        
    }
}
