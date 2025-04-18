using UnityEngine;

public class AnimationColliderUpdater : MonoBehaviour 
{
    [SerializeField] private float colliderOffsetAddon = 0.475f;
    [SerializeField] private float colliderSizeAddonX = -3f;
    [SerializeField] private float colliderSizeAddonY;

    private SpriteRenderer _renderer;
    private BoxCollider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _renderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        // Use bounds.size to get the correct size of the sprite
        _collider.size = new Vector2(_renderer.bounds.size.x + colliderSizeAddonX, _renderer.bounds.size.y + colliderSizeAddonY);

        // Calculate the offset
        var offset = _renderer.bounds.center - transform.position;
        offset.x += colliderOffsetAddon;
        offset.x *= transform.localScale.x;
        offset.y *= transform.localScale.y;
        _collider.offset = offset;
    }
}
