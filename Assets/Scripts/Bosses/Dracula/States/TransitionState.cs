using Player;
using UnityEngine;

public class TransitionState : DraculaState
{
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
        
        var teleportEnter = Boss.bossGrounded ? teleportEnterGround : teleportEnterAir;
        teleport.TeleportToLocation(teleportPosition, teleportEnter, teleportExit);
    }
    
    protected override void OnTeleportEnd()
    {
        Boss.AnimancerComponent.Play(idleAnimation);
        Boss.PauseBattle();
    }
}
