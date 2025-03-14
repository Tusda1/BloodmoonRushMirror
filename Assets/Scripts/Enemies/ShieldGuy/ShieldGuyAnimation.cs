using System;
using UnityEngine;

public class ShieldGuyAnimation : EnemyAnimationBase
{
    [SerializeField] private AnimationClip idleAnimation;
    [SerializeField] private AnimationClip attackAnimation;
    [SerializeField] private AnimationClip recoverAnimation;
    [SerializeField] private AnimationClip blockAnimation;
    [SerializeField] private AnimationClip stunHitAnimation;
    [SerializeField] private AnimationClip getStunnedAnimation;
    [SerializeField] private AnimationClip stunnedAnimation;
    [SerializeField] private AnimationClip walkAnimation;
    
    [HideInInspector] public bool isExecutingAnAction;

    public override void PlayDefaultAnimation()
    {
        animancerComponent.Play(idleAnimation);
    }

    public void Attack()
    {
        isExecutingAnAction = true;
        var state = animancerComponent.Play(attackAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            state = animancerComponent.Play(recoverAnimation);
            state.Events(this).OnEnd = () =>
            {
                state.Destroy();
                isExecutingAnAction = false;
            };
        };
    }
    
    public void Block()
    {
        isExecutingAnAction = true;
        var state = animancerComponent.Play(blockAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            isExecutingAnAction = false;
        };
    }
    
    public void StunHit()
    {
        isExecutingAnAction = true;
        var state = animancerComponent.Play(stunHitAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            isExecutingAnAction = false;
        };
    }

    public void Stun()
    {
        var state = animancerComponent.Play(getStunnedAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            animancerComponent.Play(stunnedAnimation);
        };
    }

    public void Walk()
    {
        if (animancerComponent.IsPlaying(walkAnimation)) return;
        
        animancerComponent.Play(walkAnimation);
    }
}
