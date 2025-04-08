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
        Boss.AnimancerComponent.Play(brokenAnimation);
        
        // Start the coroutine to hit the boss
        StartCoroutine(HitBossCoroutine());
    }

    private IEnumerator HitBossCoroutine()
    {
        // Move the hand towards the center of the boss face
        while (Vector2.Distance(Boss.bossCollider.bounds.center, bossHead.bossCollider.bounds.center) > explosionDistanceFromBossFace)
        {
            // Manually move the hand towards the center of the boss face
            transform.position += (Vector3)Vector2.MoveTowards(Boss.bossCollider.bounds.center, bossHead.bossCollider.bounds.center, Time.deltaTime * speed) - Boss.bossCollider.bounds.center;            
            yield return null;
        }
        
        // Play the death animation
        var state = Boss.AnimancerComponent.Play(deathAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            gameObject.SetActive(false);
            OnHandKilled?.Invoke();
        };
    }
}
