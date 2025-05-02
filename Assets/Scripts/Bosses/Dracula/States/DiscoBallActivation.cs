using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class DiscoBallActivation : DraculaState
{
    [SerializeField] private AnimationClip discoBallActivationAnimation;
    
    [HideInInspector] public DiscoBall discoBall;
    
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
        var increment = Random.Range(0, 2) == 0 ? -1 : 1;
        
        while (!safePosition)
        {
            tries++;
            
            if (tries > 100)
            {
                Debug.LogError("Could not find safe position for DiscoBallActivation");
                break;
            }
            
            // Use a boxcast to check if you are teleporting on top of the player
            var colliderSize = new Vector2(10, 3);
            var hit = Physics2D.OverlapBox(center + Vector2.up * (colliderSize.y / 2), colliderSize, 0, LayerMask.GetMask("Player"));
            safePosition = hit == null;
            
            if (!safePosition)
            {
                center += new Vector2(increment, 0);
            }
        }
        center.y += Boss.bossCollider.bounds.extents.y - Boss.bossCollider.offset.y;
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
            discoBall.EnableLaser();
            audioManager.PlayOneShotAttached("Drac_Disco_Activate", gameObject);
            EndState();
        };
    }
}
