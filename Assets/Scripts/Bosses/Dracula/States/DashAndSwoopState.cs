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
    
    private readonly Swoop _swoop = new();
    private CornerFinder _cornerFinder;
    
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
        _swoop.SetOnSwoopEnd(SwoopEnd);
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }
    
    private void OnEnable()
    {
        topLeft = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
        topLeft.x += 1f;
        topLeft.y -= 2f;
        topRight = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);
        topRight.x -= 1f;
        topRight.y -= 2f;
        bottomRight = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        bottomRight.x -= 1f;
        bottomRight.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        bottomLeft = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        bottomLeft.x += 1f;
        bottomLeft.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        
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
                if(!teleport.IsTeleporting) teleport.TeleportToLocation(teleportLocation);
                break;
            case Stage.Dashing:
                Dash();
                break;
            case Stage.Swooping:
                SwoopStage();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
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
            Boss.DisableGravity();
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

        _swoop.ExecuteSwoop(_swoopOrigin, _swoopTarget, bottomMiddle);
    }
    
    private void SwoopEnd()
    {
        _currentStage = Stage.Brain;
    }
}
