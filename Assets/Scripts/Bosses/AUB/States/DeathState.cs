using System;
using System.Collections;
using UnityEngine;

public class DeathState : BossState
{
    [SerializeField] private AnimationClip deathAnimation;
    
    private void OnEnable()
    {
        StartCoroutine(PlayOneShotDelayed("AUB_Head_Break", 1f));
        var state = Boss.AnimancerComponent.Play(deathAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            Boss.EndBattle();
            Boss.gameObject.SetActive(false);
        };
    }
}
