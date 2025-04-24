using System;
using UnityEngine;

public class GargoyleAnimations : EnemyAnimationBase
{
    [SerializeField] private AnimationClip idle;
    [SerializeField] private AnimationClip throwRock;
    
    public override void PlayDefaultAnimation()
    {
        animancerComponent.Play(idle);
    }
    
    public void PlayThrowRockAnimation(Action onEnd)
    {
        if (animancerComponent.IsPlaying(throwRock)) return;
        var state = animancerComponent.Play(throwRock);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            onEnd?.Invoke();
        };
    }
}
