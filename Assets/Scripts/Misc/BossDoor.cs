using System;
using System.Collections;
using UnityEngine;

public class BossDoor : GravityObject
{
    [SerializeField] private float closeSpeed = 5f;

    private Vector2 groundPoint;
    private Collider2D _collider;

    protected void Awake()
    {
        _collider = GetComponent<Collider2D>();
        
        // Use raycast to find groundpoint
        RaycastHit2D hit = Physics2D.Raycast(_collider.bounds.min + Vector3.down, Vector2.down, 10f, LayerMask.GetMask("Ground"));
        groundPoint = hit.point;
        groundPoint.y += _collider.bounds.extents.y + Mathf.Abs(_collider.offset.y);
    }

    public void CloseDoor()
    {
        StartCoroutine(CloseDoorCoroutine());
    }

    private IEnumerator CloseDoorCoroutine()
    {
        while (transform.position.y > groundPoint.y)
        {
            targetVelocity = Vector2.down * closeSpeed;
            yield return null;
        }
    }
}
