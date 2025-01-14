using System;
using System.Collections;
using UnityEngine;

public class DashAttackState : DraculaState
{
    [SerializeField] private int dashAmount = 5;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashCooldown = 1f;
    
    private Vector2 _dashOrigin;
    private Vector2 _dashTarget;
    private int _dashCount;
    private float _cooldownStartTime;

    private enum DashState
    {
        Teleporting,
        PreparingDash,
        Dashing,
        Cooldown
    }
    
    private DashState _dashState;

    private void OnEnable()
    {
        // Find bottom right and left corner by sending a ray straight down and then right
        var hit = Physics2D.Raycast(transform.position, Vector2.down, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find ground"); return;}
        
        var bottom = hit.point;
        bottom.y += 0.01f;
        
        hit = Physics2D.Raycast(bottom, Vector2.right, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find right corner"); return;}
        
        var rightCorner = hit.point;
        rightCorner.x -= 1f;
        rightCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        
        // Find the left corner
        hit = Physics2D.Raycast(bottom, Vector2.left, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find left corner"); return;}
        
        var leftCorner = hit.point;
        leftCorner.x += 1f;
        leftCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        
        // 50% chance to go left or right
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            _dashOrigin = rightCorner;
            _dashTarget = leftCorner;
        }
        else
        {
            _dashOrigin = leftCorner;
            _dashTarget = rightCorner;
        }
        
        Initialize();
    }

    private void Initialize()
    {
        _dashCount = 0;
        _dashState = DashState.Teleporting;
    }
    
    private void Update()
    {
        switch (_dashState)
        {
            case DashState.Teleporting:
                if (!teleport.IsTeleporting) teleport.TeleportToLocation(_dashOrigin);
                break;
            case DashState.PreparingDash:
                PrepareDash();
                break;
            case DashState.Dashing:
                Dash();
                break;
            case DashState.Cooldown:
                Cooldown();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _dashState = DashState.PreparingDash;
        Boss.EnableGravity();
    }

    private void PrepareDash()
    {
        if (!Boss.bossGrounded) return;

        var position = transform.position;
        _dashOrigin.y = position.y;
        _dashTarget.y = position.y;
        _dashState = DashState.Dashing;
    }
    
    private void Dash()
    {
        transform.position = Vector2.MoveTowards(transform.position, _dashTarget, dashSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, _dashTarget) < 0.1f)
        {
            _dashCount++;
            _dashState = DashState.Cooldown;
            _cooldownStartTime = Time.time;
        }
    }
    
    private void Cooldown()
    {
        if (_dashCount >= dashAmount)
        {
            EndState();
            Boss.DisableGravity();
            return;
        }
        
        if (Time.time - _cooldownStartTime > dashCooldown)
        {
            _dashState = DashState.Dashing;
            (_dashOrigin, _dashTarget) = (_dashTarget, _dashOrigin);
        }
    }
}
