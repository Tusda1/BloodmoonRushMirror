using System;
using System.Collections;
using UnityEngine;

public class IdleHeadState : BossState
{
    [SerializeField] private AnimationClip idleAnimation;
    [SerializeField] private AnimationClip[] faceAnimations;
    [SerializeField] private float minWaitBetweenSwap = 3f;
    [SerializeField] private float maxWaitBetweenSwap = 5f;
    
    private void OnEnable()
    {
        Boss.AnimancerComponent.Play(idleAnimation);
        
        // Start the coroutine to randomize the face
        StartCoroutine(RandomizeFace());
    }

    private void OnDisable()
    {
        // Stop the coroutine when the state is disabled
        StopAllCoroutines();
    }

    private IEnumerator RandomizeFace()
    {
        while (true)
        {
            // Wait for a random time between 1 and 3 seconds
            yield return new WaitForSeconds(UnityEngine.Random.Range(minWaitBetweenSwap, maxWaitBetweenSwap));
            
            // Randomly select a face animation
            int randomIndex = UnityEngine.Random.Range(0, faceAnimations.Length);
            Boss.AnimancerComponent.Play(faceAnimations[randomIndex]);
        }
    }
}
