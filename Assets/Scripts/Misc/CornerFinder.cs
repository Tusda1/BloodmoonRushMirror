using System;
using UnityEngine;

public class CornerFinder
{
    private Transform _transform;
    private LayerMask _groundLayer;
    private Collider2D _collider;

    public CornerFinder(Transform externalTransform, LayerMask groundLayer)
    {
        _transform = externalTransform;
        _groundLayer = groundLayer;
        _collider = _transform.GetComponent<Collider2D>();
    }

    public Vector2 GetCorner(CornerType cornerType)
    {
        Vector2 horizontalDirection = Vector2.zero;
        Vector2 verticalDirection = Vector2.zero;

        switch (cornerType)
        {
            case CornerType.TopLeft:
                horizontalDirection = Vector2.left;
                verticalDirection = Vector2.up;
                break;
            case CornerType.TopRight:
                horizontalDirection = Vector2.right;
                verticalDirection = Vector2.up;
                break;
            case CornerType.BottomLeft:
                horizontalDirection = Vector2.left;
                verticalDirection = Vector2.down;
                break;
            case CornerType.BottomRight:
                horizontalDirection = Vector2.right;
                verticalDirection = Vector2.down;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        RaycastHit2D horizontalHit = Physics2D.Raycast(_transform.position, horizontalDirection, Mathf.Infinity, _groundLayer);
        RaycastHit2D verticalHit = Physics2D.Raycast(_transform.position, verticalDirection, Mathf.Infinity, _groundLayer);
        
        var offset = new Vector2(_collider.bounds.extents.x * -horizontalDirection.x - _collider.offset.x,_collider.bounds.extents.y  * -verticalDirection.y - _collider.offset.y);

        if (horizontalHit.collider != null && verticalHit.collider != null)
        {
            return new Vector2(horizontalHit.point.x + offset.x, verticalHit.point.y + offset.y);
        }
        else
        {
            throw new Exception("Corner not found");
        }
    }

    public enum CornerType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
