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
        Dash,
        Strike
    }
    
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
            
            
        }
        else
        {
            
        }
    }
}