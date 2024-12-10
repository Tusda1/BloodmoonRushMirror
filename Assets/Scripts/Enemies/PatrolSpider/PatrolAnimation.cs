using System;
using UnityEngine;

public class PatrolAnimation : EnemyAnimationBase
{
    [SerializeField] private AnimationClip walk;
    [SerializeField] private AnimationClip windUp;
    [SerializeField] private AnimationClip attack;
    [SerializeField] private AnimationClip hitAttack;

    public override void PlayDefaultAnimation()
    {
        animancerComponent.Play(walk);
    }
    
    public void Attack(Action followUpAction)
    {
        var attackState = animancerComponent.Play(windUp);
        // When the animation ends play the attack animation
        attackState.Events(this).OnEnd = () =>
        {
            attackState = animancerComponent.Play(attack);
            attackState.Events(this).OnEnd = () =>
            {
                attackState = animancerComponent.Play(hitAttack);
                attackState.Events(this).OnEnd = () =>
                {
                    attackState.Events(this).OnEnd = null;
                    PlayDefaultAnimation();
                };
            };
        };
    }
}
