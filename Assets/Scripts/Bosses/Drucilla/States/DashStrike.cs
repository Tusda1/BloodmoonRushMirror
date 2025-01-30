using System;
using Player;
using UnityEngine;

public class DashStrike : DrucillaState
{
    [SerializeField] private float tooCloseDistance = 4f;
    
    private PlayerScript _player;
    
    private void OnEnable()
    {
        if (!_player) _player = ServiceLocator.Get<PlayerScript>();

        if (Vector2.Distance(transform.position, _player.transform.position) < tooCloseDistance)
        {
            
        }
    }
}