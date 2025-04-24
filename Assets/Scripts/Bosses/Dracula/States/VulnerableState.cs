using System;
using UnityEngine;

public class VulnerableState : DraculaState
{
    [SerializeField] private AnimationClip hurt;
    [SerializeField] private AnimationClip knockedDownAnimation;
    [SerializeField] private AnimationClip knockDownIdleAnimation;
    
    private void OnEnable()
    {
        Boss.canDoDamage = false;
        var state = Boss.AnimancerComponent.Play(hurt);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            var nextState = Boss.AnimancerComponent.Play(knockedDownAnimation);
            nextState.Events(this).OnEnd = () =>
            {
                nextState.Destroy();
                Boss.AnimancerComponent.Play(knockDownIdleAnimation);
                Boss.canTakeDamage = true;
            };
        };

        Boss.TakenDamage += TookDamage;
    }

    private void OnDisable()
    {
        Boss.TakenDamage -= TookDamage;
    }

    private void TookDamage()
    {
        audioManager.PlayOneShotAttached("Drac_Hurt", gameObject);
        Boss.canDoDamage = true;
        Boss.canTakeDamage = false;
        EndState();
    }
}
