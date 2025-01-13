using System;
using System.Collections;
using UnityEngine;

public class JumpEnemy : ReactiveBase
{
    [SerializeField] private float keepAgroDistance = 5f;
    [SerializeField] private float yPositionTolerance = 0.05f;
    
    private Rigidbody2D _playerRb2D;
    private Vector2 _lastFramePosition;
    private bool _readyToJump;
    
    protected override void Action()
    {
        if (_playerRb2D == null) _playerRb2D = targetObject.GetComponent<Rigidbody2D>();
        
        if (targetObject == null && Vector2.Distance(targetObject.position, transform.position) > keepAgroDistance)
        {
            currentState = EnemyState.Scanning;
            return;
        }

        if (Math.Abs(_lastFramePosition.y - targetObject.position.y) < yPositionTolerance)
        {
            _readyToJump = true;
        }
        
        if (Math.Abs(_lastFramePosition.y - targetObject.position.y) > yPositionTolerance && _readyToJump && isGrounded)
        {
            StartCoroutine(Jump());
        }
        
        _lastFramePosition = targetObject.position;
    }

    private IEnumerator Jump()
    {
        _readyToJump = false;
        GravityModifier = 0;
        float yDifference = targetObject.position.y - _lastFramePosition.y;

        while (yDifference > 0)
        {
            yDifference = targetObject.position.y - _lastFramePosition.y;
            
            var position = transform.position;
            transform.position = new Vector2(position.x, position.y + yDifference);
            yield return null;
        }
        
        GravityModifier = 1;
    }
}
