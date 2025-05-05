using System;
using System.Collections;
using UnityEngine;
using VContainer;
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
    
    [Inject] private CameraEffects _cameraEffects;

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
    
    private void OnDisable()
    {
        StopAllCoroutines();
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

        Boss.bossCollider.enabled = true;
        var waitOver = false;
        var state4 = Boss.AnimancerComponent.Play(headSlamAnimation);
        state4.Events(this).OnEnd = () =>
        {
            state4.Destroy();
            audioManager.PlayOneShotAttached("AUB_Head_Slam", gameObject);
            _ = _cameraEffects.ShakeCamera(0.5f, 0.5f);
            waitOver = true;
        };
        
        yield return new WaitUntil(() => waitOver);
        yield return new WaitForSeconds(postSlamWaitTime);
        
        var state2 = Boss.AnimancerComponent.Play(_currentHeadSlideAnimation);
        state2.Events(this).OnEnd = () =>
        {
            state2.Destroy();
            
            _ = _cameraEffects.ShakeCamera(0.5f, 0.5f);
            
            var state3 = Boss.AnimancerComponent.Play(_currentHeadSlideEndAnimation);
            state3.Events(this).OnEnd = () =>
            {
                state3.Destroy();
                Boss.bossCollider.enabled = false;
                EndState();
            };
        };
    }
}