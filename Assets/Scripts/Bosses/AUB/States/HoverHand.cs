using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class HoverHand : AUBHandState
{
    [SerializeField] private AnimationClip idleAnimation;
    
    [SerializeField] private float distanceFromWall = 3f;
    [SerializeField] private float maxHoverDistance = 3f;
    [SerializeField] private float minHoverDistance = 1f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float hoverSpeed = 4f;
    
    private Vector2 _hoverPosition;
    
    private CornerFinder _cornerFinder;
    
    private void Awake()
    {
        _cornerFinder = GetComponent<CornerFinder>();
    }

    private void OnEnable()
    {
        // Get position between bottom corner and top corner
        Boss.AnimancerComponent.Play(idleAnimation);
        var top = _cornerFinder.GetCorner(transform.localScale.x < 0 ? CornerFinder.CornerType.TopLeft : CornerFinder.CornerType.TopRight);
        var bottom = _cornerFinder.GetCorner(transform.localScale.x < 0 ? CornerFinder.CornerType.BottomLeft : CornerFinder.CornerType.BottomRight);
        _hoverPosition = new Vector2(top.x, bottom.y + distanceFromWall);
        _hoverPosition.y += Boss.bossCollider.bounds.center.y - transform.position.y;

        StartCoroutine(MoveToHoverPosition());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator MoveToHoverPosition()
    {
        while (Vector2.Distance(transform.position, _hoverPosition) > 1f)
        {
            Boss.Move((_hoverPosition - (Vector2)transform.position).normalized * moveSpeed);
            yield return null;
        }
        
        StartCoroutine(Hover());
    }

    private IEnumerator Hover()
    {
        while (true)
        {
            var hoverDestination = Vector2.zero;
            while (hoverDestination.magnitude > minHoverDistance)
            {
                hoverDestination = Random.insideUnitCircle * maxHoverDistance;
            }
            hoverDestination += _hoverPosition;
            
            while (Vector2.Distance(transform.position, hoverDestination) > 1f)
            {
                Boss.Move((hoverDestination - (Vector2)transform.position).normalized * hoverSpeed);
                yield return null;
            }
        }
    }
}
