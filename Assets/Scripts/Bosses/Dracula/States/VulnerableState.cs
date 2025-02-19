using System;
using UnityEngine;

public class VulnerableState : DraculaState
{
    [SerializeField] private AnimationClip knockedDownAnimation;
    [SerializeField] private AnimationClip knockDownIdleAnimation;
    [SerializeField] private AnimationClip takeDamageAnimation;
    [SerializeField] private AnimationClip recoverAnimation;
    
    private void OnEnable()
    {
        var state = Boss.AnimancerComponent.Play(knockedDownAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            Boss.AnimancerComponent.Play(knockDownIdleAnimation);
            Boss.canTakeDamage = true;
        };
    }
}
