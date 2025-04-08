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
    
    [HideInInspector] public bool isHovering;
    
    private Vector2 _hoverPosition;
    private Vector2 top;
    private Vector2 bottom;
    
    private CornerFinder _cornerFinder;
    
    private void Awake()
    {
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
        top = _cornerFinder.GetCorner(transform.localScale.x < 0 ? CornerFinder.CornerType.TopLeft : CornerFinder.CornerType.TopRight);
        bottom = _cornerFinder.GetCorner(transform.localScale.x < 0 ? CornerFinder.CornerType.BottomLeft : CornerFinder.CornerType.BottomRight);
        _hoverPosition = new Vector2(top.x + distanceFromWall * -transform.localScale.x, (bottom.y + top.y) / 2);
    }

    private void OnEnable()
    {
        Boss.AnimancerComponent.Play(idleAnimation);

        StartCoroutine(MoveToHoverPosition());
    }

    private void OnDisable()
    {
        isHovering = false;
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
        isHovering = true;
        
        while (true)
        {
            var hoverDestination = Vector2.zero;
            while (hoverDestination.magnitude > minHoverDistance)
            {
                hoverDestination = Random.insideUnitCircle * maxHoverDistance;
            }
            hoverDestination += _hoverPosition;
            
            while (Vector2.Distance(transform.position, hoverDestination) > 0.5f)
            {
                Boss.Move((hoverDestination - (Vector2)transform.position).normalized * hoverSpeed);
                yield return null;
            }

            yield return null;
        }
    }
}
