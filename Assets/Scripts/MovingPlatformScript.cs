using System;
using Player;
using UnityEngine;

public class MovingPlatformScript : GravityObject {
    #region ---Fields---
    [Header("MovingPlatform")]
    [SerializeField] private GameObject start;
    [SerializeField] private GameObject end;
    [SerializeField] private float searchRadius;
    private GravityObjectPlayerMovement _playerMovement;
    private bool _playerIsOnPlatform;
    private Vector2 _velocity = Vector2.zero;
    private Vector2 _targetPosition;
    private Vector2 _carryingPosition;
    private Vector2 _startPosition;
    private Vector2 _endPosition;
    [SerializeField] private bool _goingToStart;
    [SerializeField] private float speed;
    [SerializeField] private float smoothTime;
    [SerializeField] private BoxCollider2D collider;
    #endregion

    private void Start()
    {
        _startPosition = start.transform.position;
        _endPosition = end.transform.position;
        transform.position = _startPosition;
        start.SetActive(false);
        end.SetActive(false);
        GroundNormal = new Vector2 (0f, 1f);
    }

    protected override void ComputeVelocity() {
        var currentPosition = new Vector2(transform.position.x, transform.position.y);
        if (IsInsideCircle(_startPosition, searchRadius, currentPosition))
            _goingToStart = false;
        else if (IsInsideCircle(_endPosition, searchRadius, currentPosition)) {
            _goingToStart = true;
        }
        
        if (_goingToStart) {
            _targetPosition = _startPosition - currentPosition;
        }
        else if (!_goingToStart) {
            _targetPosition = _endPosition - currentPosition;
        }
        
        _targetPosition.Normalize();
        
        _targetPosition *= speed;
        
        _carryingPosition = Vector2.SmoothDamp(_carryingPosition, _targetPosition, ref _velocity, smoothTime);
        
        targetVelocity = _carryingPosition;
        
        if (_playerIsOnPlatform) _playerMovement.affectingVelocity = targetVelocity;
    }

    private bool IsInsideCircle(Vector2 center, float radius, Vector2 point) {
        if (Mathf.Pow((point.x - center.x), 2) + Mathf.Pow((point.y - center.y), 2) < Mathf.Pow(radius, 2)) {
            return true;
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player"))
            if (other.transform.TryGetComponent<GravityObjectPlayerMovement>(out _playerMovement)) {
                _playerIsOnPlatform = true;
                _playerMovement.MovingPlatformEnter();
            }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.transform.CompareTag("Player"))
            if (other.transform.TryGetComponent<GravityObjectPlayerMovement>(out GravityObjectPlayerMovement playerMovement)) {
                _playerIsOnPlatform = false;
                playerMovement.affectingVelocity = Vector2.zero;
                playerMovement.MovingPLatformExit();
            }
    }
}
