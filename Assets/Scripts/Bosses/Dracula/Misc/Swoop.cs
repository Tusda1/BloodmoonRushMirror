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
    
    private BossBase _externalBoss;
    private bool _swoopStarted;
    private float _swoopStartTime;
    private Action _onSwoopEnd;
    private bool _swoopStartPlayed;
    private float _timer;
    
    public void InitializeSwoop(Action onSwoopEnd, BossBase boss)
    {
        _onSwoopEnd = onSwoopEnd;
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
            return;
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

        if (!_swoopStartPlayed)
        {
            if (!_externalBoss.AnimancerComponent.IsPlaying(swoopStartAnimation))
            {
                var state = _externalBoss.AnimancerComponent.Play(swoopStartAnimation);
                state.Events(this).OnEnd += () =>
                {
                    state.Destroy();
                    _externalBoss.AnimancerComponent.Play(swoopAnimation);
                };
            }
            
            // Gradually lerp the rotation of the external transform to face the direction it is moving
            _swoopStartTime = Time.time;
            float animationDuration = swoopStartAnimation.length / 2;
            Debug.Log("swoop start animation is playing");
        
            _externalBoss.transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, Vector2.SignedAngle(Vector2.up, position - (Vector2)_externalBoss.transform.position), _timer / animationDuration));
        
            _timer += Time.deltaTime;

            if (_timer > animationDuration)
            {
                _swoopStartPlayed = true;
                _timer = 0;
            }
            else
            {
                return;
            }
        }
        
        _externalBoss.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, position - (Vector2)_externalBoss.transform.position));
        
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
