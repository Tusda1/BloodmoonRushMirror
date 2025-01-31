using System;
using Player;
using UnityEngine;

public class DashStrike : DrucillaState
{
    [SerializeField] private float tooCloseDistance = 4f;
    
    private Vector2 _teleportLocation;
    
    private PlayerScript _player;
    private CornerFinder _cornerFinder;
    
    private enum DashStrikeStage
    {
        Teleport,
        LightningDash,
        Strike
    }
    
    private DashStrikeStage _stage;
    
    private void OnEnable()
    {
        if (!_player) _player = ServiceLocator.Get<PlayerScript>();

        if (Vector2.Distance(transform.position, _player.transform.position) < tooCloseDistance)
        {
            var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
            var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);

            if (Vector2.Distance(leftCorner, _player.transform.position) < Vector2.Distance(rightCorner, _player.transform.position))
            {
                leftCorner.x += 1;
                leftCorner.y -= 2;
                _teleportLocation = leftCorner;
            }
            else
            {
                rightCorner.x -= 1;
                rightCorner.y -= 2;
                _teleportLocation = rightCorner;
            }
            
            _stage = DashStrikeStage.Teleport;
        }
        else
        {
            _stage = DashStrikeStage.LightningDash;
        }
    }

    private void Update()
    {
        switch (_stage)
        {
            case DashStrikeStage.Teleport:
                if (!teleport.IsTeleporting) teleport.TeleportToLocation(_teleportLocation);
                break;
            case DashStrikeStage.LightningDash:
                LightningDash();
                break;
            case DashStrikeStage.Strike:
                Strike();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _stage = DashStrikeStage.LightningDash;
    }
    
    private void LightningDash()
    {
        
    }
    
    private void Strike()
    {
        
    }
}