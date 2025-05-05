using System;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class GroundSlamState : DrucillaState
{
    [SerializeField] private float teleportDistance = 6f;
    
    private bool _hasTeleported;
    
    [SerializeField] private GroundSlam groundSlam = new();
    private PlayerScript _player;
    private Collider2D _collider;
    
    private void Start()
    { 
        _player = ServiceLocator.Get<PlayerScript>();
        _collider = GetComponent<Collider2D>();

        groundSlam.InitializeGroundSlam(_collider, Boss.AnimancerComponent);
    }

    private void OnDisable()
    {
        _hasTeleported = false;
        groundSlam.ResetGroundSlam();
    }

    private void Update()
    {
        if (!_hasTeleported)
        {
            if (teleport.IsTeleporting) return;
            
            teleport.TeleportToLocation(_player.transform.position + Vector3.up * teleportDistance, DrucillaReference.TeleportEnter, DrucillaReference.TeleportExit);
        }
        else
        {
            groundSlam.ExecuteGroundSlam(ref Boss.targetVelocity, GroundSlamEnd);
        }
    }
    
    private void GroundSlamEnd()
    {
        EndState();
    }
    
    protected override void OnTeleportEnd()
    {
        _hasTeleported = true;
    }
}
