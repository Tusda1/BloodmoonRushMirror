using System;
using UnityEngine;

public class DiscoBallActivation : DraculaState
{
    [SerializeField] private AnimationClip discoBallActivationAnimation;
    [SerializeField] private DiscoBall discoBall;
    
    private CornerFinder _cornerFinder;
    
    protected override void Awake()
    {
        base.Awake();
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }

    private void OnEnable()
    {
        var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        
        // Go center
        var center = (rightCorner + leftCorner) / 2;
        
        var teleportEnter = Boss.isGrounded ? teleportEnterGround : teleportEnterAir;
        teleport.TeleportToLocation(center, teleportEnter, teleportExit);
    }

    protected override void OnTeleportEnd()
    {
        var state = Boss.AnimancerComponent.Play(discoBallActivationAnimation);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            Boss.AnimancerComponent.Play(idleAnimation);
            discoBall.Enable();
            EndState();
        };
    }
}
