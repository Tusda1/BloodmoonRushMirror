using System;
using UnityEngine;

public class ColliderSizeMatcher : MonoBehaviour
{
    [SerializeField] private BoxCollider2D matchingCollider;
    private BoxCollider2D collider2D;
    
    private void Awake()
    {
        collider2D = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        collider2D.size = matchingCollider.size;
    }
}
