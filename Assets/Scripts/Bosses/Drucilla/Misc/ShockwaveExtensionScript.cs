using System;
using Animancer;
using FMODUnity;
using UnityEngine;

public class ShockwaveExtensionScript : MonoBehaviour 
{
    [SerializeField] private float shockWaveMoveDistance = 4f;
    [SerializeField] private AnimationClip shockwaveAnimation;
    private AnimancerComponent _animancerComponent;
    private Vector2 direction;
    
    private void Start()
    {
        _animancerComponent = GetComponent<AnimancerComponent>();
        direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        var newPos = transform.position + (Vector3)direction * 4f;
        // Use overlapPoint to check if the new position is inside a wall
        var hit = Physics2D.OverlapPoint(newPos + Vector3.up, LayerMask.GetMask("Ground"));
        if (!hit)
        {
            transform.position = newPos;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!_animancerComponent.IsPlaying(shockwaveAnimation))
        {
            RuntimeManager.PlayOneShotAttached("event:/Sfx/Boss/Drucilla/Druc_shockwave", gameObject);
            var state = _animancerComponent.Play(shockwaveAnimation);
            state.Events(this).OnEnd = () =>
            {
                // Use raycast to check if there is a wall in localscale.x direction
                var hit = Physics2D.Raycast(transform.position + Vector3.up * 0.1f, direction, shockWaveMoveDistance + 1f, LayerMask.GetMask("Ground"));
                if (hit.collider != null)
                {
                    state.Destroy();
                    _animancerComponent.Stop();
                    Destroy(gameObject);
                }
                else
                {
                    state.Destroy();
                    _animancerComponent.Stop();
                    transform.position += (Vector3)direction * shockWaveMoveDistance;
                }
            };
        }
    }
}
