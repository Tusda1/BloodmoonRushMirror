using System;
using Player;
using UnityEngine;

public class DefeatState : DrucillaState
{
    [SerializeField] private AnimationClip defeatAnimation;
    
    private void OnEnable()
    {
        // Teleport close to the player on the ground and then play the crying animation
        var player = ServiceLocator.Get<PlayerScript>();
        var playerPosition = player.transform.position;
        Vector2 teleportPosition;
        // Check if the teleport position is valid using raycast (not colliding with the ground)
        var raycastHit = Physics2D.Raycast(new Vector2(playerPosition.x, playerPosition.y + 0.1f), Vector2.right, 7f, LayerMask.GetMask("Ground"));
        if (!raycastHit)
        {
            teleportPosition = new Vector2(playerPosition.x + 5f, playerPosition.y);
        }
        else
        {
            teleportPosition = new Vector2(playerPosition.x - 5f, playerPosition.y);
        }
        
        teleportPosition.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        
        teleport.TeleportToLocation(teleportPosition, DrucillaReference.TeleportEnter, DrucillaReference.TeleportExit);
    }

    protected override void OnTeleportEnd()
    {
        Boss.AnimancerComponent.Play(defeatAnimation);
        Boss.EndBattle();
    }

    public void TeleportAway()
    {
        teleport.TeleportToLocation(new Vector2(-100, 200), DrucillaReference.TeleportEnter, DrucillaReference.TeleportExit);
    }
}
