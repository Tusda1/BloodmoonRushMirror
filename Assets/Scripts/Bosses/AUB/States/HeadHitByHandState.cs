using System;
using System.Collections;
using UnityEngine;

public class HeadHitByHandState : BossState
{
    [SerializeField] private AnimationClip hitAnimation;
    [SerializeField] private AnimationClip explodeAnimation;
    
    private int _hitCount;

    private void OnEnable()
    {
        StartCoroutine(PlayHitAnimation());
    }
    
    private void OnDisable()
    {
        // Stop the coroutine when the state is disabled
        StopAllCoroutines();
    }

    private IEnumerator PlayHitAnimation()
    {
        _hitCount++;
        
        if (_hitCount == 1)
        {
            // Play the hit animation
            Boss.AnimancerComponent.Play(hitAnimation);

            yield return new WaitForSeconds(1f);
            
            EndState();
        }
        else if (_hitCount == 2)
        {
            // Play the explode animation
            var state = Boss.AnimancerComponent.Play(explodeAnimation);
            state.Events(this).OnEnd = () =>
            {
                state.Destroy();
                EndState();
            };
        }
    }
}
