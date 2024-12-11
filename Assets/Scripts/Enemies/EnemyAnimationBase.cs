using System;
using UnityEngine;
using Animancer;

public class EnemyAnimationBase : MonoBehaviour 
{
    [SerializeField] private AnimationClip deathAnimation;
    [SerializeField] private AnimationClip hitAnimation;

    protected AnimancerComponent animancerComponent { get; private set; }
    
    protected virtual void Awake()
    {
        animancerComponent = GetComponent<AnimancerComponent>();
    }

    protected virtual void OnEnable()
    {
        PlayDefaultAnimation();
    }

    public virtual void PlayDefaultAnimation()
    {
        // Play the default animation
    }
    
    public void PlayHitAnimation()
    {
        if (animancerComponent || hitAnimation == null)
        {
            return;
        }
        
        animancerComponent.Play(hitAnimation);
    }
    
    public void PlayDeathAnimation(Action onDeathAnimationEnd)
    {
        if (deathAnimation == null)
        {
            onDeathAnimationEnd?.Invoke();
            return;
        }
        
        var deathState = animancerComponent.Play(deathAnimation);
        deathState.Events(this).OnEnd = () =>
        {
            onDeathAnimationEnd?.Invoke();
        };
    }
    
}
