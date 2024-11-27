using System;
using UnityEngine;
using UnityEngine.Serialization;

public class SwoopAttackState : BossState
{
    [SerializeField] private float swoopDuration = 2f;
    [SerializeField] private float easePower = 2f;
    
    private Vector2 _swoopOrigin;
    private Vector2 _swoopTarget;
    private Vector2 _swoopArcBottom;
    private float swoopStartTime;
    private SwoopState _swoopState;

    private Teleport _teleport;
    
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
        // Set variables
        Boss.gravity = 0;
        
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

    private void OnDisable()
    {
        Boss.gravity = 1;
    }

    private void Update()
    {
        switch (_swoopState)
        {
            case SwoopState.Teleporting:
                if (!_teleport.IsTeleporting) _teleport.TeleportToLocation(_swoopOrigin);
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
    
    private void OnTeleportEnd()
    {
        SwoopStart();
    }
    
    private void SwoopStart()
    {
        _swoopState = SwoopState.Swooping;
        swoopStartTime = Time.time;
    }
    
    private void Swoop()
    {
        // Calculate t with easing for smooth acceleration and deceleration
        float t = (Time.time - swoopStartTime) / swoopDuration;
    
        // Clamp t to the range [0, 1]
        if (t > 1f)
        {
            _swoopState = SwoopState.Shooting;
            return;
        }

        // Apply easing using easePower
        if (t < 0.5f)
        {
            t = Mathf.Pow(t * 2, easePower) / 2f; // Ease-in
        }
        else
        {
            t = 1 - Mathf.Pow(2 * (1 - t), easePower) / 2f; // Ease-out
        }

        // Calculate the position using the eased t
        Vector2 position = CalculateBezierPoint(t, _swoopOrigin, _swoopTarget, _swoopArcBottom);
        transform.position = position;
    }

    private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p2, Vector2 peak)
    {
        // Calculate the control point p1 based on the desired peak
        Vector2 p1 = 2 * peak - 0.5f * p0 - 0.5f * p2;

        // Standard quadratic Bézier curve calculation
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
