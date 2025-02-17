using System;
using UnityEngine;

public class PatrolAnimation : EnemyAnimationBase
{
    [SerializeField] private AnimationClip walk;
    [SerializeField] private AnimationClip windUp;
    [SerializeField] private AnimationClip attack;

    [HideInInspector] public bool isAttacking;

    public override void PlayDefaultAnimation()
    {
        animancerComponent.Play(walk);
        isAttacking = false;
    }
    
    public void Attack(Action followUpAction)
    {
        isAttacking = true;
        
        var attackState = animancerComponent.Play(windUp);
        // When the animation ends play the attack animation
        attackState.Events(this).OnEnd = () =>
        {
            followUpAction?.Invoke();
            attackState = animancerComponent.Play(attack);
            attackState.Events(this).OnEnd = () =>
            {
                attackState.Events(this).OnEnd = null;
                PlayDefaultAnimation();
            };
        };
    }
}
