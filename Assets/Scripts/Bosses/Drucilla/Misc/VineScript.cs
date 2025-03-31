using System;
using System.Collections;
using Animancer;
using UnityEngine;

public class VineScript : MonoBehaviour
{
    [SerializeField] private AnimationClip vineLure;
    [SerializeField] private AnimationClip vineAttack;
    [SerializeField] private AnimationClip vineIdle;
    [SerializeField] private AnimationClip vineWithdraw;
    
    [SerializeField] private Collider2D positionCollider;
    [SerializeField] private float lureWaitTime = 1f;
    [SerializeField] private float activeTime = 3f;
    
    private float _pivotToGroundDistance;
    private AnimancerComponent _animancerComponent;
    private Collider2D _collider;

    private void Awake()
    {
        _animancerComponent = GetComponent<AnimancerComponent>();
        _collider = GetComponent<Collider2D>();
        _collider.enabled = false;
    }

    private void OnEnable()
    {
        // Send raycast down to find the ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            Debug.LogError("VineScript: No ground found");
            return;
        }
        
        _pivotToGroundDistance = Mathf.Abs(positionCollider.bounds.min.y - hit.point.y); // Calculate the distance from the pivot to the ground
        
        // Set the bottom of the position collider to the ground
        positionCollider.transform.position = new Vector2(transform.position.x, hit.point.y + _pivotToGroundDistance);

        StartCoroutine(Attack());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Attack()
    {
        _animancerComponent.Play(vineLure);
        
        yield return new WaitForSeconds(lureWaitTime);

        var state = _animancerComponent.Play(vineAttack);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            _animancerComponent.Play(vineIdle);
        };
        
        _collider.enabled = true;

        yield return new WaitForSeconds(activeTime);
        
        var state2 = _animancerComponent.Play(vineWithdraw);
        state2.Events(this).OnEnd = () =>
        {
            state2.Destroy();
            _collider.enabled = false;
            gameObject.SetActive(false);
        };
    }
}
