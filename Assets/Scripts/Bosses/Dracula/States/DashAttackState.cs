using System;
using System.Collections;
using UnityEngine;

public class DashAttackState : BossState
{
    [SerializeField] private int dashAmount = 5;
    [SerializeField] private float dashSpeed = 10f;
    
    private Vector2 _dashOrigin;
    private Vector2 _dashTarget;
    private int _dashCount;
    private bool _isDashing;
    
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
        
        // Find the left corner
        hit = Physics2D.Raycast(bottom, Vector2.left, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find left corner"); return;}
        
        var leftCorner = hit.point;
        leftCorner.x += 1f;
        
        // 50% chance to go left or right
        if (UnityEngine.Random.value > 0.5f)
        {
            _dashOrigin = rightCorner;
            _dashTarget = leftCorner;
        }
        else
        {
            _dashOrigin = leftCorner;
            _dashTarget = rightCorner;
        }
    }
    
    private void Update()
    {
        if (_dashCount < dashAmount && !_isDashing)
        {
            StartCoroutine(Dash());
        }
    }
    
    private IEnumerator Dash()
    {
        _isDashing = true;
        _dashCount++;
        
        
        
        yield return null;
    }
}
