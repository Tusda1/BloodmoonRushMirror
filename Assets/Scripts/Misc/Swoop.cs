using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Swoop
{
    [SerializeField] private float easePower = 2f;
    [SerializeField] private float swoopDuration;
    
    private bool _swoopStarted;
    private float _swoopStartTime;
    private Action _onSwoopEnd;
    
    public void SetOnSwoopEnd(Action onSwoopEnd)
    {
        _onSwoopEnd = onSwoopEnd;
    }
    
    public Vector2 ExecuteSwoop(Vector2 swoopOrigin, Vector2 swoopTarget, Vector2 swoopArcBottom)
    {
        if (!_swoopStarted)
        {
            _swoopStarted = true;
            _swoopStartTime = Time.time;
        }
        
        // Calculate t with easing for smooth acceleration and deceleration
        float t = (Time.time - _swoopStartTime) / swoopDuration;
    
        // Clamp t to the range [0, 1]
        if (t > 1f)
        {
            _onSwoopEnd();
            return swoopTarget;
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
        return position;
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
}
