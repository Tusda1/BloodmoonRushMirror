using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class DanceDash : DraculaState
{
    [SerializeField] private int minDashAmount = 1;
    [SerializeField] private int maxDashAmount = 3;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashCooldown = 1f;
    
    private Vector2 _dashOrigin;
    private Vector2 _dashTarget;
    private int _dashCount;
    private int _dashAmount;
    private float _waitStartTime;
    
    private CornerFinder _cornerFinder;
    
    private enum DanceDashStage
    {
        Teleport,
        Dash,
        Wait
    }
    
    private DanceDashStage _danceDashStage;

    protected override void Awake()
    {
        base.Awake();
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }

    private void OnEnable()
    {
        // Randomize the amount of dashes
        _dashAmount = Random.Range(minDashAmount, maxDashAmount + 1);
        
        var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        rightCorner.x -= 1f;
        rightCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        leftCorner.x += 1f;
        leftCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        
        // 50% chance to go left or right
        if (Random.Range(0, 2) == 0)
        {
            _dashOrigin = rightCorner;
            _dashTarget = leftCorner;
        }
        else
        {
            _dashOrigin = leftCorner;
            _dashTarget = rightCorner;
        }
        
        _danceDashStage = DanceDashStage.Teleport;
    }

    private void Update()
    {
        switch (_danceDashStage)
        {
            case DanceDashStage.Teleport:
                if (!teleport.IsTeleporting) teleport.TeleportToLocation(_dashOrigin);
                break;
            case DanceDashStage.Dash:
                Dash();
                break;
            case DanceDashStage.Wait:
                Wait();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _danceDashStage = DanceDashStage.Dash;
        Boss.EnableGravity();
    }
    
    private void Dash()
    {
        transform.position = Vector2.MoveTowards(transform.position, _dashTarget, dashSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, _dashTarget) < 0.1f)
        {
            _dashCount++;
            _danceDashStage = DanceDashStage.Wait;
            _waitStartTime = Time.time;
        }
    }
    
    private void Wait()
    {
        if (_dashCount >= _dashAmount)
        {
            EndState();
            Boss.DisableGravity();
            return;
        }
        
        if (Time.time - _waitStartTime > dashCooldown)
        {
            _danceDashStage = DanceDashStage.Dash;
            (_dashOrigin, _dashTarget) = (_dashTarget, _dashOrigin);
        }
    }
}
