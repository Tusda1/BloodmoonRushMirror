using System;
using UnityEngine;

public class DeathState : BossState
{
    [SerializeField] private AnimationClip deathAnimation;
    
    private void OnEnable()
    {
        audioManager.PlayOneShotAttached("AUB_Head_Break", gameObject);
        var state = Boss.AnimancerComponent.Play(deathAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            Boss.EndBattle();
            Boss.gameObject.SetActive(false);
        };
    }
}
