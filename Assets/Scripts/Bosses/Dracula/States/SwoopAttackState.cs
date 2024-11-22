using System;
using UnityEngine;
using UnityEngine.Serialization;

public class SwoopAttackState : BossState
{
    [SerializeField] private float swoopDuration = 2f;
    [SerializeField] private float swoopStartTime;
    
    private Vector2 _swoopOrigin;
    private Vector2 _swoopTarget;
    private Vector2 _swoopArcBottom;
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
        
        // Send a ray from exactly between the two corners to find the arc bottom
        _swoopArcBottom = (rightCorner + leftCorner) / 2;
        hit = Physics2D.Raycast(_swoopArcBottom, Vector2.down, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find arc bottom"); return;}
        
        _swoopArcBottom = hit.point + Vector2.up;
        
        _swoopState = SwoopState.Teleporting;
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
        SwoopStart();
    }
    
    private void SwoopStart()
    {
        _swoopState = SwoopState.Swooping;
        swoopStartTime = Time.time;
    }
    
    private void Swoop()
    {
        float t = (Time.time - swoopStartTime) / swoopDuration;
        if (t > 1f)
        {
            t = 1f;
            _swoopState = SwoopState.Shooting;
        }

        Vector2 position = CalculateBezierPoint(t, _swoopOrigin, _swoopArcBottom, _swoopTarget);
        transform.position = position;
    }

    private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector2 p = uu * p0; // (1-t)^2 * p0
        p += 2 * u * t * p1; // 2 * (1-t) * t * p1
        p += tt * p2; // t^2 * p2

        return p;
    }
    
    private void Shoot()
    {
        // Shoot a projectile at the player
    }
}
