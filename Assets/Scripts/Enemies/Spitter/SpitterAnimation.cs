using System;
using Animancer;
using UnityEngine;
using UnityEngine.Serialization;

public class SpitterAnimation : EnemyAnimationBase
{
    [SerializeField] private AnimationClip spitWindUp;
    [SerializeField] private AnimationClip spit;
    [SerializeField] private AnimationClip idle;

    [HideInInspector] public bool isAttacking;
    
    public override void PlayDefaultAnimation()
    {
        animancerComponent.Play(idle);
        isAttacking = false;
    }

    public void Attack(Action followUpAction)
    {
        isAttacking = true;
        var attackState = animancerComponent.Play(spitWindUp);
        // When the animation ends play the spit animation
        attackState.Events(this).OnEnd = () =>
        {
            followUpAction?.Invoke();
            isAttacking = false;
            attackState = animancerComponent.Play(spit);
            attackState.Events(this).OnEnd = () =>
            {
                PlayDefaultAnimation();
                attackState.Events(this).OnEnd = null;
            };
        };
    }
}
