using System;
using UnityEngine;
using VContainer;

public class SlimeAnimation : EnemyAnimationBase
{
    [SerializeField] private AnimationClip idle;
    [SerializeField] private AnimationClip jump;
    [SerializeField] private AnimationClip up;
    [SerializeField] private AnimationClip fallStart;
    [SerializeField] private AnimationClip fall;
    [SerializeField] private AnimationClip land;
    [SerializeField] private AnimationClip split;
    [SerializeField] private AnimationClip splitFall;
    [SerializeField] private AnimationClip splitLand;
    [SerializeField] private AnimationClip waitForReform; 
    [SerializeField] private AnimationClip reform;
    
    [SerializeField] private string jumpSoundID = "Slime_jump";
    [SerializeField] private string landSoundID = "Slime_land";
    [SerializeField] private string reformSoundID = "Slime_reform";
    
    [Inject] private IAudioManager _audioManager;

    public override void PlayDefaultAnimation()
    {
        animancerComponent.Play(idle);
    }
    
    public void Jump()
    {
        _audioManager.PlayOneShotAttached(jumpSoundID, gameObject);
        var state = animancerComponent.Play(jump);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            state = animancerComponent.Play(up);
        };
    }
    
    public void Fall(GravityObject gravityObject)
    {
        var state = animancerComponent.Play(fallStart);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            state = animancerComponent.Play(fall);
            state.Events(this).OnEnd = () =>
            {
                if (gravityObject.isGrounded)
                {
                    state.Destroy();
                    state = animancerComponent.Play(land);
                    _audioManager.PlayOneShotAttached(landSoundID, gameObject);
                    state.Events(this).OnEnd = () =>
                    {
                        state.Destroy();
                        PlayDefaultAnimation();
                    };
                }
            };
        };
    }
    
    public void Split(GravityObject gravityObject)
    {
        var state = animancerComponent.Play(split);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            state = animancerComponent.Play(splitFall);
            state.Events(this).OnEnd = () =>
            {
                if (gravityObject.isGrounded)
                {
                    state.Destroy();
                    state = animancerComponent.Play(splitLand);
                    state.Events(this).OnEnd = () =>
                    {
                        state.Destroy();
                        animancerComponent.Play(waitForReform);
                    };
                }
            };
        };
    }
    
    public void Reform(Action onReformEnd)
    {
        _audioManager.PlayOneShotAttached(reformSoundID, gameObject);
        var state = animancerComponent.Play(reform);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            onReformEnd?.Invoke();
            PlayDefaultAnimation();
        };
    }
}
