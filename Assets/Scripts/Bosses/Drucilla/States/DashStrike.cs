using System;
using System.Collections;
using Player;
using UnityEngine;

public class DashStrike : DrucillaState
{
    [SerializeField] private float tooCloseDistance = 4f;
    
    private Vector2 _teleportLocation;
    private bool _lightningDashStarted;
    
    private PlayerScript _player;
    private CornerFinder _cornerFinder;
    private LightningManager _lightningManager = new();
    
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
            var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
            var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);

            if (Vector2.Distance(leftCorner, _player.transform.position) < Vector2.Distance(rightCorner, _player.transform.position))
            {
                leftCorner.x += 1;
                leftCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
                _teleportLocation = leftCorner;
            }
            else
            {
                rightCorner.x -= 1;
                rightCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
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
        if (!_lightningDashStarted)
        {
            _lightningDashStarted = true;
            _lightningManager.CreateLightning(transform.position, _player.transform.position);
        }
    }
    
    private void Strike()
    {
        
    }
}