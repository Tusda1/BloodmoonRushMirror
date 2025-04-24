using UnityEngine;

public class AnimationColliderUpdater : MonoBehaviour 
{
    [SerializeField] protected float colliderOffsetAddon = 0.475f;
    [SerializeField] protected float colliderSizeAddonX = -3f;
    [SerializeField] protected float colliderSizeAddonY;

    protected SpriteRenderer objectRenderer;
    protected BoxCollider2D objectCollider;

    private void Awake()
    {
        objectCollider = GetComponent<BoxCollider2D>();
        objectRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        SetColliderSize();
        SetColliderOffset();
    }

    protected virtual void SetColliderSize()
    {
        // Use bounds.size to get the correct size of the sprite
        objectCollider.size = new Vector2(objectRenderer.bounds.size.x + colliderSizeAddonX, objectRenderer.bounds.size.y + colliderSizeAddonY);
    }
    
    protected virtual void SetColliderOffset()
    {
        // Calculate the offset
        var offset = objectRenderer.bounds.center - transform.position;
        offset.x += colliderOffsetAddon;
        offset.x *= transform.localScale.x;
        offset.y *= transform.localScale.y;
        objectCollider.offset = offset;
    }
}
