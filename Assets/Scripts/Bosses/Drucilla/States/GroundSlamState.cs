using System;
using Player;
using UnityEngine;

public class GroundSlamState : DrucillaState
{
    [SerializeField] private float teleportDistance = 6f;
    
    private bool _hasTeleported;
    
    private PlayerScript _player;
    private Collider2D _collider;
    private GroundSlam _groundSlam;
    
    private void Start()
    { 
        _player = ServiceLocator.Get<PlayerScript>();
        _collider = GetComponent<Collider2D>();

        _groundSlam = new GroundSlam(_collider);
    }

    private void Update()
    {
        if (!_hasTeleported)
        {
            if (teleport.IsTeleporting) return;
            
            teleport.TeleportToLocation(_player.transform.position + Vector3.up * teleportDistance);
        }
        else
        {
            _groundSlam.ExecuteGroundSlam(ref Boss.targetVelocity);
        }
    }
}
