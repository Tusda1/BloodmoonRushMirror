using System;
using UnityEngine;

public class DiscoBallActivation : DraculaState
{
    [SerializeField] private AnimationClip discoBallActivationAnimation;
    [SerializeField] private DiscoBall discoBall;
    
    private CornerFinder _cornerFinder;
    private Collider2D _collider;
    
    protected override void Awake()
    {
        base.Awake();
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
        _collider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        
        // Go center
        var center = (rightCorner + leftCorner) / 2;
        var safePosition = false;
        var tries = 0;
        
        while (!safePosition)
        {
            tries++;
            
            if (tries > 100)
            {
                Debug.LogError("Could not find safe position for DiscoBallActivation");
                break;
            }
            
            // Use a boxcast to check if you are teleporting on top of the player
            var hit = Physics2D.BoxCast(center, _collider.bounds.size, 0, Vector2.zero, 0, LayerMask.GetMask("Player"));
            safePosition = hit.collider == null;
            
            if (!safePosition)
            {
                center += Vector2.right;
            }
        }
        
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
