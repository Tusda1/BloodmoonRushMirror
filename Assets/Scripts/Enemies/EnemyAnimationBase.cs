using System;
using UnityEngine;
using Animancer;
using VContainer;

public class EnemyAnimationBase : MonoBehaviour
{
    [SerializeField] private AnimationClip deathAnimation;
    [SerializeField] private AnimationClip hitAnimation;

    [Inject] protected IAudioManager audioManager;
    
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
    
    public void PlayDeathAnimation(float delay, Action onDeathAnimationEnd)
    {
        if (deathAnimation == null)
        {
            onDeathAnimationEnd?.Invoke();
            return;
        }
        
        var deathState = animancerComponent.Play(deathAnimation);
        // if (deathState.Events(this, out var events))
        // {
        //     events.Add(delay, dropRubies);
        // }
        
        deathState.Events(this).OnEnd = () =>
        {
            onDeathAnimationEnd?.Invoke();
            deathState.Events(this).OnEnd = null;
        };
    }
}
