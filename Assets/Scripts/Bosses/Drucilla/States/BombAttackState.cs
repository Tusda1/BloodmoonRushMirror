using System;
using Player;
using UnityEngine;

public class BombAttackState : DrucillaState
{
    [SerializeField] private GameObject bombPrefab;
    
    private Vector2 _teleportPosition;
    
    private PlayerScript _player;
    private Pooler _bombPooler;
    
    private enum BombAttackStage
    {
        Teleport,
        DropBombs,
        GroundSlam
    }
    
    private BombAttackStage _stage;

    private void Start()
    {
        _player = ServiceLocator.Get<PlayerScript>();
        
        _bombPooler = TryGetCommunityPooler.TryGetPooler(bombPrefab);
    }

    private void OnEnable()
    {
        Boss.GravityModifier = 0;
        
        var hit = Physics2D.Raycast(_player.transform.position, Vector2.up, 100f, LayerMask.GetMask("Ground"));
        
        _teleportPosition = hit.point + Vector2.down * 2.5f;
    }

    private void Update()
    {
        switch (_stage)
        {
            case BombAttackStage.Teleport:
                if (!teleport.IsTeleporting) teleport.TeleportToLocation(_teleportPosition);
                break;
            case BombAttackStage.DropBombs:
                DropBombs();
                break;
            case BombAttackStage.GroundSlam:
                GroundSlam();
                break;
        }
    }

    private void DropBombs()
    {
        var bomb = _bombPooler.GetPooledObject();
        bomb.transform.position = transform.position;
        bomb.SetActive(true);
    }
    
    private void GroundSlam()
    {
        
    }
}
