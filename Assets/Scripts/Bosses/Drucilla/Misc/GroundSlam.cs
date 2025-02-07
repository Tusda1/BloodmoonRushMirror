using System;
using UnityEngine;

public class GroundSlam 
{
    private int _groundSlamStage;
    private const float UpSpeed = 20f;
    private const float UpTime = 0.1f;
    private const float DownWaitTime = 0.3f;
    private const float DownSpeed = 15f;
    private float _timer;
    
    private readonly Collider2D _collider;
    
    public GroundSlam(Collider2D externalCollider)
    {
        _collider = externalCollider;
    }

    public void ExecuteGroundSlam(ref Vector2 velocityReference, Action onGroundSlamEnd = null)
    {
        switch (_groundSlamStage)
        {
            case 0:
                _timer += Time.deltaTime;
                if (_timer < UpTime)
                {
                    velocityReference.y = UpSpeed;
                }
                else
                {
                    _groundSlamStage++;
                    _timer = 0;
                }
                break;
            
            case 1:
                _timer += Time.deltaTime;
                if (_timer > DownWaitTime)
                {
                    if (_collider.IsTouchingLayers(LayerMask.GetMask("Ground")))
                    {
                        velocityReference.y = 0;
                        _groundSlamStage++;
                        _timer = 0;
                    }
                    else
                    {
                        velocityReference.y = -DownSpeed;
                    }
                }
                break;
            
            case 2:
                // TODO : Add explosion type thing when slamming
                onGroundSlamEnd?.Invoke();
                _groundSlamStage = 0;
                break;
            
            default:
                Debug.LogError("GroundSlamStage is out of bounds");
                break;
        }
    }
}
