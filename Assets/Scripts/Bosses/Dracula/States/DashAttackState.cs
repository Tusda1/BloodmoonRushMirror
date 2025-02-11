using System;
using System.Collections;
using UnityEngine;

public class DashAttackState : DraculaState
{
    [SerializeField] private AnimationClip dashAnimation;
    [SerializeField] private int dashAmount = 5;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashCooldown = 1f;
    
    private Vector2 _dashOrigin;
    private Vector2 _dashTarget;
    private int _dashCount;
    private float _cooldownStartTime;
    private bool _isDashing;
    
    private CornerFinder _cornerFinder;

    private enum DashState
    {
        Teleporting,
        PreparingDash,
        Dashing,
        Cooldown
    }
    
    private DashState _dashState;

    protected override void Awake()
    {
        base.Awake();
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }

    private void OnEnable()
    {
        var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        rightCorner.x -= 2f;
        rightCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        leftCorner.x += 2f;
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
                if (!teleport.IsTeleporting)
                {
                    var teleportEnter = Boss.isGrounded ? teleportEnterGround : teleportEnterAir;
                    teleport.TeleportToLocation(_dashOrigin, teleportEnter, teleportExit);
                }
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
        Boss.AnimancerComponent.Play(idleAnimation);
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
        if (!_isDashing)
        {
            _isDashing = true;
            Boss.AnimancerComponent.Play(dashAnimation);
        }
        
        var direction = _dashTarget.x > _dashOrigin.x ? Vector2.right : Vector2.left;
        Boss.targetVelocity = direction * dashSpeed;
        // Check if the boss crossed the finish line
        if (transform.position.x * direction.x > _dashTarget.x * direction.x)
        {
            _dashCount++;
            _dashState = DashState.Cooldown;
            _cooldownStartTime = Time.time;
            _isDashing = false;
            Boss.AnimancerComponent.Play(idleAnimation);
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
