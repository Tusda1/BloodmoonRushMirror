using System;
using UnityEngine;

public class BombAttackState : BossState
{
    [SerializeField] private GameObject bombPrefab;
    
    private Teleport _teleport;
    
    private enum BombAttackStage
    {
        Position,
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
            case BombAttackStage.Position:
                
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
