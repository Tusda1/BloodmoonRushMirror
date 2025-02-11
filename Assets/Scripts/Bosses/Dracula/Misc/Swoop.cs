using System;
using Animancer;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Swoop
{
    [SerializeField] private float easePower = 2f;
    [SerializeField] private float swoopDuration;
    [SerializeField] private AnimationClip swoopStartAnimation;
    [SerializeField] private AnimationClip swoopAnimation;
    
    private AnimancerComponent _animancer;
    private BossBase _externalBoss;
    private bool _swoopStarted;
    private float _swoopStartTime;
    private Action _onSwoopEnd;
    private bool _swoopStartPlayed;
    
    public void InitializeSwoop(Action onSwoopEnd, AnimancerComponent animancer, BossBase boss)
    {
        _onSwoopEnd = onSwoopEnd;
        _animancer = animancer;
        _externalBoss = boss;
    }
    
    public void ExecuteSwoop(Vector2 swoopOrigin, Vector2 swoopTarget, Vector2 swoopArcBottom)
    {
        if (!_swoopStarted)
        {
            // Set the localscale of the external transform to face the direction it is moving
            _externalBoss.facingDirection = swoopTarget.x > swoopOrigin.x ? Vector2.right : Vector2.left;
            _swoopStarted = true;
            _swoopStartTime = Time.time;
        }
        
        // Calculate t with easing for smooth acceleration and deceleration
        float t = (Time.time - _swoopStartTime) / swoopDuration;
    
        // Clamp t to the range [0, 1]
        if (t > 1f)
        {
            _onSwoopEnd();
            _swoopStarted = false;
            _externalBoss.transform.position = swoopTarget;
            ResetSwoop();
        }

        // Apply easing using easePower
        if (t < 0.5f)
        {
            t = Mathf.Pow(t * 2, easePower) / 2f; // Ease-in
        }
        else
        {
            t = 1 - Mathf.Pow(2 * (1 - t), easePower) / 2f; // Ease-out
        }
        
        // Calculate the position using the eased t
        Vector2 position = CalculateBezierPoint(t, swoopOrigin, swoopTarget, swoopArcBottom);
        
        // Play the swoop start animation once
        if (!_swoopStartPlayed && !_animancer.IsPlaying(swoopStartAnimation))
        {
            var state = _animancer.Play(swoopStartAnimation);
            state.Events(this).OnEnd += () =>
            {
                Debug.Log("Swoop start animation ended");
                state.Destroy();
                _animancer.Play(swoopAnimation);
                _swoopStartPlayed = true;
            };
        }
        else
        {
            // Rotate the external transform to face the direction it is moving
            _externalBoss.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(_externalBoss.facingDirection, position - (Vector2)_externalBoss.transform.position));
        }
        
        _externalBoss.transform.position = position;
    }

    private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p2, Vector2 peak)
    {
        // Calculate the control point p1 based on the desired peak
        Vector2 p1 = 2 * peak - 0.5f * p0 - 0.5f * p2;

        // Standard quadratic Bézier curve calculation
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector2 p = uu * p0; // (1-t)^2 * p0
        p += 2 * u * t * p1; // 2 * (1-t) * t * p1
        p += tt * p2; // t^2 * p2

        return p;
    }
    
    private void ResetSwoop()
    {
        _swoopStarted = false;
        _swoopStartPlayed = false;
        _externalBoss.transform.rotation = Quaternion.identity;
    }
}
