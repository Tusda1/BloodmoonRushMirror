using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class HeadSlide : BossState
{
    [SerializeField] private AnimationClip idle;
    [SerializeField] private AnimationClip headSlamAnimation;
    [SerializeField] private AnimationClip warningAnimationRight;
    [SerializeField] private AnimationClip warningAnimationLeft;
    [SerializeField] private AnimationClip headSlideAnimationRight;
    [SerializeField] private AnimationClip headSlideEndAnimationRight;
    [SerializeField] private AnimationClip headSlideAnimationLeft;
    [SerializeField] private AnimationClip headSlideEndAnimationLeft;
    
    [SerializeField] private float postWarningWaitTime = 4f;
    [SerializeField] private float postSlamWaitTime = 2f;
    
    private AnimationClip _currentWarningAnimation;
    private AnimationClip _currentHeadSlideAnimation;
    private AnimationClip _currentHeadSlideEndAnimation;

    private void OnEnable()
    {
        if (Random.Range(0, 2) == 0)
        {
            _currentWarningAnimation = warningAnimationRight;
            _currentHeadSlideAnimation = headSlideAnimationRight;
            _currentHeadSlideEndAnimation = headSlideEndAnimationRight;
        }
        else
        {
            _currentWarningAnimation = warningAnimationLeft;
            _currentHeadSlideAnimation = headSlideAnimationLeft;
            _currentHeadSlideEndAnimation = headSlideEndAnimationLeft;
        }
        
        StartCoroutine(HeadSlideCoroutine());
    }

    private IEnumerator HeadSlideCoroutine()
    {
        var state = Boss.AnimancerComponent.Play(_currentWarningAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            Boss.AnimancerComponent.Play(idle);
        };
        
        yield return new WaitForSeconds(state.Duration + postWarningWaitTime);
        
        Boss.AnimancerComponent.Play(headSlamAnimation);
        
        yield return new WaitForSeconds(headSlamAnimation.length + postSlamWaitTime);
        
        var state2 = Boss.AnimancerComponent.Play(_currentHeadSlideAnimation);
        state2.Events(this).OnEnd = () =>
        {
            state2.Destroy();
            Boss.AnimancerComponent.Play(_currentHeadSlideEndAnimation);
            EndState();
        };
    }
}