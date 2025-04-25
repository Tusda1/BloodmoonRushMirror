using System;
using Animancer;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;

[Serializable]
public class Swoop
{
    [SerializeField] public float swoopDuration;
    [SerializeField] private float easePower = 2f;
    [SerializeField] private AnimationClip swoopStartAnimation;
    [SerializeField] private AnimationClip swoopAnimation;
    
    public float t;
    private BossBase _externalBoss;
    private bool _swoopStarted;
    private float _swoopStartTime;
    private Action _onSwoopEnd;
    private bool _swoopStartPlayed;
    
    public void InitializeSwoop(Action onSwoopEnd, BossBase boss)
    {
        _onSwoopEnd = onSwoopEnd;
        _externalBoss = boss;
    }
    
    public void ExecuteSwoop(Vector2 swoopOrigin, Vector2 swoopTarget, Vector2 swoopArcBottom, bool playAnimation = true, float completionLevel = 1f)
    {
        if(Time.deltaTime == 0)
        {
            return;
        }
        
        if (!_swoopStarted)
        {
            // Set the localscale of the external transform to face the direction it is moving
            _externalBoss.facingDirection = swoopTarget.x > swoopOrigin.x ? Vector2.right : Vector2.left;
            _swoopStarted = true;
            _swoopStartTime = Time.time;
            RuntimeManager.PlayOneShotAttached("event:/Sfx/Boss/Dracula/Drac_Swoop", _externalBoss.gameObject);
        }

        if (!_swoopStartPlayed && playAnimation)
        {
            _externalBoss.AnimancerComponent.Play(swoopAnimation);
            _swoopStartPlayed = true;
        }
        
        // Calculate t with easing for smooth acceleration and deceleration
        t = (Time.time - _swoopStartTime) / swoopDuration;
    
        // Clamp t to the range [0, 1]
        if (t > completionLevel)
        {
            _onSwoopEnd();
        }

        if (t > 1)
        {
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
    
    public void ResetSwoop()
    {
        _swoopStarted = false;
        _swoopStartPlayed = false;
    }
}
