using System;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class HandSlamLogic
{
    [SerializeField] private AnimationClip slamAnimation;
    [SerializeField] private AnimationClip slamHitAnimation;
    [SerializeField] private float slamSpeed = 20f;
    
    private BossBase _boss;
    private CameraEffects _cameraEffects;
    
    public void Initialize(BossBase boss, CameraEffects cameraEffects)
    {
        _boss = boss;
        _cameraEffects = cameraEffects;
    }
    
    public bool ExecuteHandSlamLogic()
    {
        if (!_boss.AnimancerComponent.IsPlaying(slamAnimation))
        {
            _boss.AnimancerComponent.Play(slamAnimation);
        }
        
        RaycastHit2D hit = Physics2D.Raycast(_boss.bossCollider.bounds.center, Vector2.down, _boss.bossCollider.bounds.extents.y + 0.1f, LayerMask.GetMask("Ground"));
        
        if (!_boss.bossCollider.IsTouchingLayers(LayerMask.GetMask("Ground")) || hit.collider == null)
        {
            // Move the hand downwards
            _boss.Move(Vector2.down * slamSpeed);
            return false;
        }
        
        _boss.AnimancerComponent.Play(slamHitAnimation);
        _ = _cameraEffects.ShakeCamera(0.5f, 0.5f);
        return true;
    }
}
