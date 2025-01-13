using System;
using System.Collections;
using Player;
using UnityEngine;

public class JumpEnemy : ReactiveBase
{
    [SerializeField] private float keepAgroDistance = 5f;
    [SerializeField] private float yPositionTolerance = 0.05f;
    [SerializeField] private float jumpModifier = 1f;
    
    private GravityObjectPlayerMovement _playerMovement;
    private Vector2 _lastFramePosition;
    private bool _readyToJump;
    private float initialGravityModifier;
    
    protected override void Action()
    {
        if (_playerMovement == null) _playerMovement = targetObject.GetComponent<GravityObjectPlayerMovement>();
        
        if (targetObject == null && Vector2.Distance(targetObject.position, transform.position) > keepAgroDistance)
        {
            currentState = EnemyState.Scanning;
            return;
        }

        if (targetObject.position.y - _lastFramePosition.y > yPositionTolerance && !_playerMovement.isClimbing)
        {
            _readyToJump = true;
        }
        
        if (_readyToJump && isGrounded)
        {
            StartCoroutine(Jump());
        }
        
        _lastFramePosition = targetObject.position;
    }

    private IEnumerator Jump()
    {
        _readyToJump = false;
        initialGravityModifier = GravityModifier;
        GravityModifier = 0;
        float yDifference = targetObject.position.y - _lastFramePosition.y;

        while (yDifference > 0)
        {
            yDifference = targetObject.position.y - _lastFramePosition.y;
            
            var position = transform.position;
            transform.position = new Vector2(position.x, position.y + yDifference * jumpModifier);
            yield return new WaitForFixedUpdate();
        }
        
        GravityModifier = initialGravityModifier;
    }
}
