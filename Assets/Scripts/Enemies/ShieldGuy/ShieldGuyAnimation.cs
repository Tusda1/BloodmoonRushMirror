using System;
using UnityEngine;
using VContainer;

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
    
    [Inject] private IAudioManager _audioManager;
    [SerializeField] private string blockSoundID = "Shield_Block";

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
        _audioManager.PlayOneShotAttached(blockSoundID, gameObject);
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
        isExecutingAnAction = false;
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
