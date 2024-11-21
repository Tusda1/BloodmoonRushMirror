using System;
using UnityEngine;

public class SwoopAttackState : BossState
{
    private Vector2 _swoopOrigin;
    private Vector2 _swoopTarget;
    private SwoopState _swoopState;
    
    private Teleport _teleport;
    private bool _isTeleporting;
    
    private enum SwoopState 
    {
        Teleporting,
        Swooping,
        Shooting,
        Finished
    }

    private void Start()
    {
        _teleport = new Teleport(transform, Boss.AnimancerComponent, Boss.TeleportEnter, Boss.TeleportExit, OnTeleportEnd);
    }

    private void OnEnable()
    {
        // Send a ray straight up and find the roof
        var hit = Physics2D.Raycast(transform.position, Vector2.up, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find roof"); return;}
        
        var roof = hit.point;
        roof.y -= 0.1f;
        
        // Find the right corner
        hit = Physics2D.Raycast(roof, Vector2.right, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find right corner"); return;}
        
        Vector2 rightCorner = hit.point;
        rightCorner.x -= 1f;
        rightCorner.y -= 2f;
        
        // Find the left corner
        hit = Physics2D.Raycast(roof, Vector2.left, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find left corner"); return;}
        
        Vector2 leftCorner = hit.point;
        leftCorner.x += 1f;
        leftCorner.y -= 2f;
        
        // 50% chance to go left or right
        if (UnityEngine.Random.value > 0.5f)
        {
            _swoopOrigin = rightCorner;
            _swoopTarget = leftCorner;
        }
        else
        {
            _swoopOrigin = leftCorner;
            _swoopTarget = rightCorner;
        }
    }
    
    private void Update()
    {
        switch (_swoopState)
        {
            case SwoopState.Teleporting:
                if (!_isTeleporting) TeleportToOrigin();
                break;
            case SwoopState.Swooping:
                Swoop();
                break;
            case SwoopState.Shooting:
                Shoot();
                break;
            case SwoopState.Finished:
                break;
        }
    }
    
    private void TeleportToOrigin()
    {
        _teleport.TeleportToLocation(_swoopOrigin);
        _isTeleporting = true;
    }
    
    private void OnTeleportEnd()
    {
        _isTeleporting = false;
        _swoopState = SwoopState.Swooping;
    }
    
    private void Swoop()
    {
        // Teleport to the origin
        transform.position = _swoopOrigin;
        
        // Swoop to the target
        _swoopState = SwoopState.Swooping;
    }
    
    private void Shoot()
    {
        // Shoot a projectile at the player
    }
}
