using System;
using Animancer;
using UnityEngine;

public class ShockwaveExtensionScript : MonoBehaviour 
{
    [SerializeField] private float shockWaveMoveDistance = 4f;
    [SerializeField] private AnimationClip shockwaveAnimation;
    private AnimancerComponent _animancerComponent;
    
    private void Awake()
    {
        _animancerComponent = GetComponent<AnimancerComponent>();
        var direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        transform.position += (Vector3)direction * 4f;
        
        var state = _animancerComponent.Play(shockwaveAnimation);
        state.Events(this).OnEnd = () =>
        {
            // Use raycast to check if there is a wall in localscale.x direction
            
            var hit = Physics2D.Raycast(transform.position + Vector3.up * 0.1f, direction, shockWaveMoveDistance + 1f, LayerMask.GetMask("Ground"));
            if (hit.collider != null)
            {
                state.Destroy();
                Destroy(gameObject);
            }
            else
            {
                transform.position += (Vector3)direction * shockWaveMoveDistance;
            }
        };
    }
}
