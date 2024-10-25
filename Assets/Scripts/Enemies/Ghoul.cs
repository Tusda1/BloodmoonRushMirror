using UnityEngine;

public class Ghoul : ReactiveBase
{
    [SerializeField] private float detectionDistance = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float universalAgroDistance = 2f;
    
    private bool _isJumping;
    
    private void Charge()
    {
        targetVelocity = facingDirection * moveSpeed;
        
        // Detect if the Ghoul charges into a wall
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, 1.5f, EnemyAndGroundLayerMask);
        if (hit.collider != null)
        {
            // If the Ghoul charges into a wall, stop charging and start scanning
            targetVelocity = Vector2.zero;
                
            currentState = EnemyState.Scanning;
        }
            
        if (!_isJumping)
        {
            hit = Physics2D.Raycast(GetBottomPosition() + facingDirection, Vector2.down, 1f, LayerMask.GetMask("Ground"));
            if (hit.collider == null)
            {
                // If the ghoul is about to fall off a ledge, jump off the ledge
                velocity.y = jumpForce;
                _isJumping = true;
            }
        }
        
        else if (isGrounded)
        {
            _isJumping = false;
        }
    }
    
    private void CheckForPlayer()
    {
        // Check for a player in close proximity
        Collider2D hitClose = Physics2D.OverlapCircle(transform.position, universalAgroDistance, LayerMask.GetMask("Player"));
        if (hitClose != null)
        {
            // Return the direction to the player
            facingDirection = (hitClose.transform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
            currentState = EnemyState.Moving;
            return;
        }

        // Cast a ray to the right
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, detectionDistance, LayerMask.GetMask("Player"));
        if (hitRight.collider != null)
        {
            // Return the direction to the player
            facingDirection = Vector2.right;
            currentState = EnemyState.Moving;
            return;
        }

        // Cast a ray to the left
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, detectionDistance, LayerMask.GetMask("Player"));
        if (hitLeft.collider != null)
        {
            // Return the direction to the player
            facingDirection = Vector2.left;
            currentState = EnemyState.Moving;
        }
    }
}
