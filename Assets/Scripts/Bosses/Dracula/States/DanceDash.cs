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
    
    private Teleport _teleport;
    private readonly TeleportConditionalActivation _teleportConditionalActivation = new();
    
    private enum DanceDashStage
    {
        Teleport,
        Dash,
        Wait
    }
    
    private DanceDashStage _danceDashStage;

    private void Start()
    {
        _teleport = new Teleport(transform, Boss.AnimancerComponent, Boss.TeleportEnter, Boss.TeleportExit, OnTeleportEnd);
    }

    private void OnEnable()
    {
        // Randomize the amount of dashes
        _dashAmount = Random.Range(minDashAmount, maxDashAmount + 1);
        
        // Find bottom right and left corner by sending a ray straight down and then right
        var hit = Physics2D.Raycast(transform.position, Vector2.down, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            Debug.LogError("Couldn't find ground");
            return;
        }

        var bottom = hit.point;
        bottom.y += 0.01f;

        hit = Physics2D.Raycast(bottom, Vector2.right, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            Debug.LogError("Couldn't find right corner");
            return;
        }

        var rightCorner = hit.point;
        rightCorner.x -= 1f;
        rightCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);

        // Find the left corner
        hit = Physics2D.Raycast(bottom, Vector2.left, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            Debug.LogError("Couldn't find left corner");
            return;
        }

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
        
        _danceDashStage = DanceDashStage.Teleport;
    }

    private void Update()
    {
        switch (_danceDashStage)
        {
            case DanceDashStage.Teleport:
                if (!_teleport.IsTeleporting) _teleportConditionalActivation.ActivateTeleport(transform.position, _dashOrigin, OnTeleportEnd, _teleport.TeleportToLocation);
                break;
            case DanceDashStage.Dash:
                Dash();
                break;
            case DanceDashStage.Wait:
                Wait();
                break;
        }
    }
    
    private void OnTeleportEnd()
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
