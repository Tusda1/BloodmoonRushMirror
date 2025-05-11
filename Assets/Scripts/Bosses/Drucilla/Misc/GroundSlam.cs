using System;
using Animancer;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[Serializable]
public class GroundSlam 
{
    [SerializeField] private AnimationClip groundSlamEnter;
    [SerializeField] private AnimationClip groundSlamDown;
    [SerializeField] private AnimationClip groundSlamSlam;
    [SerializeField] private AnimationClip groundSlamStuck;
    [SerializeField] private AnimationClip groundSlamExit;
    [SerializeField] private AnimationClip groundSlamEnded;
    [SerializeField] private AnimationClip idleAnimation;
    
    [SerializeField] private float upSpeed = 20f;
    [SerializeField] private float downWaitTime = 0.3f;
    [SerializeField] private float downSpeed = 15f;
    [SerializeField] private float stuckWaitTime = 1.5f;
    [SerializeField] private float waitBeforeNextAttack = 0.5f;
    
    private int _groundSlamStage;
    private float _timer;
    
    private GameObject _shockwavePrefab;
    private Collider2D _collider;
    private AnimancerComponent _animancerComponent;
    
    public void InitializeGroundSlam(Collider2D externalCollider, AnimancerComponent externalAnimancerComponent)
    {
        _collider = externalCollider;
        _animancerComponent = externalAnimancerComponent;
        _shockwavePrefab = Resources.Load<GameObject>("Prefabs/DrucillaShockwave");
    }

    public void ExecuteGroundSlam(ref Vector2 velocityReference, Action onGroundSlamEnd = null)
    {
        switch (_groundSlamStage)
        {
            case 0:

                if (!_animancerComponent.IsPlaying(groundSlamEnter))
                {
                    RuntimeManager.PlayOneShotAttached("event:/Sfx/Boss/Drucilla/Druc_groundslam_start", _animancerComponent.gameObject);
                    var state = _animancerComponent.Play(groundSlamEnter);
                    state.Events(this).OnEnd ??= () =>
                    {
                        state.Destroy();
                        _animancerComponent.Play(groundSlamDown);
                        _groundSlamStage++;
                    };
                }
                
                //velocityReference.y = upSpeed;
                break;
            
            case 1:
                if (_collider.IsTouchingLayers(LayerMask.GetMask("Ground")))
                {
                    if (!_animancerComponent.IsPlaying(groundSlamSlam))
                    {
                        StartShockwave();
                        RuntimeManager.PlayOneShotAttached("event:/Sfx/Boss/Drucilla/Druc_groundslam_impact", _animancerComponent.gameObject);
                        var state = _animancerComponent.Play(groundSlamSlam);
                        state.Events(this).OnEnd = () =>
                        {
                            state.Destroy();
                            _animancerComponent.Play(groundSlamStuck);
                            _groundSlamStage++;
                        };
                    }
                    velocityReference.y = 0;
                }
                else 
                { 
                    velocityReference.y = -downSpeed;
                }
                break;
            
            case 2:
                if (_timer >= stuckWaitTime)
                {
                    if (!_animancerComponent.IsPlaying(groundSlamExit) && !_animancerComponent.IsPlaying(groundSlamEnded))
                    {
                        var state = _animancerComponent.Play(groundSlamExit);
                        state.Events(this).OnEnd = () =>
                        {
                            state.Destroy();
                            
                            var newState = _animancerComponent.Play(groundSlamEnded);
                            newState.Events(this).OnEnd = () =>
                            {
                                newState.Destroy();
                                _groundSlamStage++;
                                _timer = 0;
                                _animancerComponent.Play(idleAnimation);
                            };
                        };
                    }
                }
                else
                {
                    _timer += Time.deltaTime;
                }
                break;
            
            case 3:
                if (_timer >= waitBeforeNextAttack)
                {
                    ResetGroundSlam();
                    onGroundSlamEnd?.Invoke();
                }
                else
                {
                    _timer += Time.deltaTime;
                }
                
                break;
            
            default:
                Debug.LogError("GroundSlamStage is out of bounds");
                break;
        }
    }
    
    public void ResetGroundSlam()
    {
        _groundSlamStage = 0;
        _timer = 0;
    }

    private void StartShockwave()
    {
        // Send a raycast from the center of the collider to the ground
        RaycastHit2D hit = Physics2D.Raycast(_collider.bounds.center, Vector2.down, 100, LayerMask.GetMask("Ground"));
        
        Object.Instantiate(_shockwavePrefab, hit.point, Quaternion.identity);
    }
}
