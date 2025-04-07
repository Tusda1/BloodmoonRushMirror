using System;
using System.Collections;
using UnityEngine;

public class HandSlide : AUBHandState
{
    [SerializeField] private AnimationClip slamAnimation;
    [SerializeField] private AnimationClip slideAnimation;
    [SerializeField] private float slideSpeed = 5f;
    [SerializeField] private float slamSpeed = 10f;
    [SerializeField] private float waitTimeAfterWallHit = 1f;

    private void OnEnable()
    {
        StartCoroutine(Slam());
    }

    private IEnumerator Slam()
    {
        Boss.AnimancerComponent.Play(slamAnimation);
        while (!Boss.bossCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            Boss.Move(Vector2.down * slamSpeed);
            yield return null;
        }

        StartCoroutine(Slide());
    }

    private IEnumerator Slide()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + 0.1f);
        Boss.AnimancerComponent.Play(slideAnimation);
        while (!Boss.bossCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            Boss.Move(Vector2.left * (transform.localScale.x * slideSpeed));
            yield return null;
        }

        yield return new WaitForSeconds(waitTimeAfterWallHit);
        
        Boss.StateMachine.ForceSetState(hoverHand);
    }
}
