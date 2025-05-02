using Animancer;
using UnityEngine;

public class FloatingCandleScript : GravityObject
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private AnimationClip flameAnimation;
    [SerializeField] private SmoothDampScript smoothDampScript = new();
    
    private Vector2 _targetPosition;
    private Vector2 _initialPosition;
    
    private AnimancerComponent _animancer;

    private new void Awake()
    {
        base.Awake();
        GravityModifier = 0f;
        _animancer = GetComponent<AnimancerComponent>();
        _animancer.Play(flameAnimation);
        _initialPosition = transform.position;
    }

    protected override void ComputeVelocity()
    {
        var currentPosition = transform.position;
        if (_targetPosition == Vector2.zero || Vector2.Distance(currentPosition, _targetPosition) < 0.1f)
        {
            _targetPosition = new Vector2(_initialPosition.x, _initialPosition.y + Random.Range(-0.5f, 0.5f));
        }
        
        smoothDampScript.SmoothDampVector2((_targetPosition - (Vector2)currentPosition) * speed, ref targetVelocity);
    }
}
