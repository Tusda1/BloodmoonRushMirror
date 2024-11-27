using System;
using Animancer;
using UnityEngine;
using UnityEngine.Serialization;

public class SpitterAnimation : MonoBehaviour
{
    [SerializeField] private AnimationClip attackWindUp;
    [SerializeField] private AnimationClip attack;
    [SerializeField] private AnimationClip idle;
    
    private AnimancerComponent _animancerComponent;
    
    private void Awake()
    {
        _animancerComponent = GetComponent<AnimancerComponent>();
    }

    private void Start()
    {
        _animancerComponent.Play(idle);
    }

    public void Attack(Action followUpAction)
    {
        var attackState = _animancerComponent.Play(attackWindUp);
        // When the animation ends play the spit animation
        attackState.Events(this).OnEnd = () =>
        {
            _animancerComponent.Play(attack);
            attackState.Events(this).OnEnd = () =>
            {
                followUpAction?.Invoke();
                _animancerComponent.Play(idle);
            };
        };
    }
}
