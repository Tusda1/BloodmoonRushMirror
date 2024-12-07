using System;
using UnityEngine;
using UnityEngine.Serialization;

public class DashAndSwoopState : DraculaState
{
    private Vector2 bottomRight;
    private Vector2 bottomLeft;
    private Vector2 topRight;
    private Vector2 topLeft;
    private Vector2 bottomMiddle;
    private Vector2 teleportLocation;
    
    [SerializeField] private float dashSpeed = 10f;
    private Vector2 _dashTarget;
    private bool _dashStarted;
    
    private bool _swoopStarted;
    private Vector2 _swoopOrigin;
    private Vector2 _swoopTarget;
    
    [SerializeField] private Swoop swoop = new();
    private Teleport _teleport;
    
    private enum Stage
    {
        Brain,
        Teleporting,
        Dashing,
        Swooping
    }
    
    private Stage _stage;
    private Stage _currentStage;

    private void Start()
    {
        _teleport = new Teleport(transform, Boss.AnimancerComponent, Boss.TeleportEnter, Boss.TeleportExit, OnTeleportEnd);
        swoop.SetOnSwoopEnd(SwoopEnd);
    }
    
    private void OnEnable()
    {
        // Find all 4 corners of the room
        var hit = Physics2D.Raycast(transform.position, Vector2.down, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find ground"); return;}
        
        var ground = hit.point;
        ground.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        
        hit = Physics2D.Raycast(ground, Vector2.right, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find right wall"); return;}
        
        bottomRight = hit.point;
        bottomRight.x -= 1f;
        
        hit = Physics2D.Raycast(ground, Vector2.left, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find left wall"); return;}
        
        bottomLeft = hit.point;
        bottomLeft.x += 1f;
        
        hit = Physics2D.Raycast(ground, Vector2.up, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find roof"); return;}
        
        var roof = hit.point;
        roof.y -= 2f;
        
        // Find the right corner
        hit = Physics2D.Raycast(roof, Vector2.right, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find right corner"); return;}
        
        topRight = hit.point;
        topRight.x -= 1f;
        
        // Find the left corner
        hit = Physics2D.Raycast(roof, Vector2.left, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find left corner"); return;}
        
        topLeft = hit.point;
        topLeft.x += 1f;
        
        bottomMiddle = (bottomRight + bottomLeft) / 2;
        
        _currentStage = Stage.Brain;
    }

    private void Update()
    {
        switch (_currentStage)
        {
            case Stage.Brain:
                Calculate();
                break;
            case Stage.Teleporting:
                if(!_teleport.IsTeleporting) _teleport.TeleportToLocation(teleportLocation);
                break;
            case Stage.Dashing:
                Dash();
                break;
            case Stage.Swooping:
                SwoopStage();
                break;
        }
    }
    
    private void OnTeleportEnd()
    {
        _currentStage = _stage;
    }

    private void Calculate()
    {
        // 50% chance to do a dash or a swoop
        if (UnityEngine.Random.value > 0.5f)
        {
            _stage = Stage.Dashing;
            // 50% chance to teleport left or right
            teleportLocation = UnityEngine.Random.value > 0.5f ? bottomRight : bottomLeft;
        }
        else
        {
            _stage = Stage.Swooping;
            // 50% chance to teleport left or right
            teleportLocation = UnityEngine.Random.value > 0.5f ? topRight : topLeft;
        }
        
        _currentStage = Stage.Teleporting;
    }
    
    private void Dash()
    {
        if (!_dashStarted)
        {
            _dashStarted = true;
            Boss.EnableGravity();
            _dashTarget = teleportLocation == bottomRight ? bottomLeft : bottomRight;
        }
        
        if (Boss.bossGrounded) return;
        
        transform.position = Vector2.MoveTowards(transform.position, _dashTarget, dashSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, _dashTarget) < 0.1f)
        {
            _currentStage = Stage.Brain;
        }
    }
    
    private void SwoopStage()
    {
        if (!_swoopStarted)
        {
            if (teleportLocation == topRight)
            {
                _swoopOrigin = topRight;
                _swoopTarget = topLeft;
            }
            else
            {
                _swoopOrigin = topLeft;
                _swoopTarget = topRight;
            }
            
            _swoopStarted = true;
        }

        swoop.ExecuteSwoop(_swoopOrigin, _swoopTarget, bottomMiddle);
    }
    
    private void SwoopEnd()
    {
        _currentStage = Stage.Brain;
    }
}
