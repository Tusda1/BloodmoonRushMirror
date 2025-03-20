using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ClimbingSpider : EnemyBase
{
    [SerializeField] private AnimationClip climbingAnimation;
    [SerializeField] private AnimationClip idleAnimation;
    [SerializeField] private float waitTime = 3f;
    
    public Vector2 topPosition;
    public Vector2 bottomPosition;
    
    public event Action OnSpiderDestroyed;

    private void OnEnable()
    {
        StartCoroutine(GoDown());
    }
    
    private IEnumerator GoDown()
    {
        while(transform.position.y > bottomPosition.y)
        {
            transform.position = Vector2.MoveTowards(transform.position, bottomPosition, 0.1f);
            yield return null;
        }

        yield return new WaitForSeconds(waitTime);
        
        StartCoroutine(GoUp());
    }
    
    private IEnumerator GoUp()
    {
        while(transform.position.y < topPosition.y)
        {
            transform.position = Vector2.MoveTowards(transform.position, topPosition, 0.1f);
            yield return null;
        }

        yield return new WaitForSeconds(waitTime);
        
        StartCoroutine(GoDown());
    }

    private void OnDestroy()
    {
        OnSpiderDestroyed?.Invoke();
    }
}
