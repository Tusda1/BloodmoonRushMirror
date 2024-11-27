using System;
using Animancer;
using UnityEngine;
using UnityEngine.Serialization;

public class SpitterAnimation : MonoBehaviour
{
    [SerializeField] private AnimationClip spitWindUp;
    [SerializeField] private AnimationClip spit;
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
        var attackState = _animancerComponent.Play(spitWindUp);
        // When the animation ends play the spit animation
        attackState.Events(this).OnEnd = () =>
        {
            followUpAction?.Invoke();
            attackState = _animancerComponent.Play(spit);
            attackState.Events(this).OnEnd = () =>
            {
                attackState.Events(this).OnEnd = null;
                _animancerComponent.Play(idle);
            };
        };
    }
}
