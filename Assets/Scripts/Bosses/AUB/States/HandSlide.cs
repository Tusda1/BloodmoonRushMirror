using System;
using System.Collections;
using UnityEngine;
using VContainer;

[DefaultExecutionOrder(1)]
public class HandSlide : AUBHandState
{
    [SerializeField] private AnimationClip slideAnimation;
    [SerializeField] private float slideSpeed = 5f;
    [SerializeField] private float waitTimeAfterWallHit = 1f;
    [SerializeField] private Vector2 shakeIntensity = new(0.5f, 0.5f);
    
    [SerializeField] private HandSlamLogic handSlamLogic = new();
    [Inject] private CameraEffects _cameraEffects;

    private void OnEnable()
    {
        handSlamLogic.Initialize(Boss, _cameraEffects);
        
        StartCoroutine(Slam());
    }
    
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Slam()
    {
        while (!handSlamLogic.ExecuteHandSlamLogic())
        {
            yield return null;
        }

        StartCoroutine(Slide());
    }

    private IEnumerator Slide()
    {
        Boss.AnimancerComponent.Play(slideAnimation);
        while (!IsTouchingGround())
        {
            Boss.Move(Vector2.left * (transform.localScale.x * slideSpeed));
            yield return null;
        }
        
        audioManager.PlayOneShotAttached("AUB_Hand_Slam", gameObject);
        _ = _cameraEffects.ShakeCamera(shakeIntensity.x, shakeIntensity.y);

        yield return new WaitForSeconds(waitTimeAfterWallHit);
        
        Boss.StateMachine.ForceSetState(hoverHand);
    }
    
    private bool IsTouchingGround()
    {
        // Cast a ray downwards from the center of the collider
        var direction = transform.localScale.x < 0 ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(Boss.bossCollider.bounds.center, direction, Boss.bossCollider.bounds.extents.y + 0.1f, LayerMask.GetMask("Ground", "EnemyCollider"));
        return hit.collider != null;
    }
}
