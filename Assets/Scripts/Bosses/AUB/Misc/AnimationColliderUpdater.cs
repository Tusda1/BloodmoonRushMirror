using UnityEngine;

public class AnimationColliderUpdater : MonoBehaviour 
{
    [SerializeField] private float colliderOffsetAddon = 0.475f;

    private SpriteRenderer _headRenderer;
    private BoxCollider2D _headCollider;

    private void Awake()
    {
        _headCollider = GetComponent<BoxCollider2D>();
        _headRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        _headCollider.size = new Vector2(_headRenderer.size.x - 3, _headRenderer.size.y);
        var offset = _headRenderer.bounds.center - transform.position;
        offset.x += colliderOffsetAddon;
        _headCollider.offset = offset;
    }
}
