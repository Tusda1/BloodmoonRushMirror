using System;
using UnityEngine;

public class SlimeAnimation : EnemyAnimationBase
{
    [SerializeField] private AnimationClip idle;
    [SerializeField] private AnimationClip jumpStart;
    [SerializeField] private AnimationClip jumpEnd;
    [SerializeField] private AnimationClip up;
    [SerializeField] private AnimationClip fallStart;
    [SerializeField] private AnimationClip fall;
    [SerializeField] private AnimationClip land;
    [SerializeField] private AnimationClip split;
    [SerializeField] private AnimationClip splitFall;
    [SerializeField] private AnimationClip splitLand;
    [SerializeField] private AnimationClip waitForReform; 
    [SerializeField] private AnimationClip reform;

    public override void PlayDefaultAnimation()
    {
        animancerComponent.Play(idle);
    }
    
    public void Jump()
    {
        var state = animancerComponent.Play(jumpStart);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            state = animancerComponent.Play(jumpEnd);
            state.Events(this).OnEnd = () =>
            {
                state.Destroy();
                state = animancerComponent.Play(up);
            };
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
        animancerComponent.
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
        var state = animancerComponent.Play(reform);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            onReformEnd?.Invoke();
            PlayDefaultAnimation();
        };
    }
}
