using System;
using System.Collections;
using UnityEngine;

public class HandKilledState : BossState
{
    [SerializeField] private AnimationClip brokenAnimation;
    [SerializeField] private AnimationClip deathAnimation;
    [SerializeField] private BossBase bossHead;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float explosionDistanceFromBossFace = 2f;
    
    public event Action OnHandKilled;

    private void OnEnable()
    {
        audioManager.PlayOneShotAttached("AUB_Hand_Break", gameObject);
        Boss.AnimancerComponent.Play(brokenAnimation);
        
        // Start the coroutine to hit the boss
        StartCoroutine(HitBossCoroutine());
    }
    
    private void OnDisable()
    {
        // Stop the coroutine when the state is disabled
        StopAllCoroutines();
    }

    private IEnumerator HitBossCoroutine()
    {
        // Move the hand towards the center of the boss face
        while (Vector2.Distance(Boss.bossCollider.bounds.center, bossHead.transform.position + Vector3.up * 11) > explosionDistanceFromBossFace)
        {
            // Manually move the hand towards the center of the boss face
            transform.position += (Vector3)Vector2.MoveTowards(Boss.bossCollider.bounds.center, bossHead.transform.position + Vector3.up * 11, Time.fixedDeltaTime * speed) - Boss.bossCollider.bounds.center;      
            yield return new WaitForFixedUpdate();
        }
        
        // Play the death animation
        OnHandKilled?.Invoke();
        var state = Boss.AnimancerComponent.Play(deathAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            gameObject.SetActive(false);
        };
    }
}
