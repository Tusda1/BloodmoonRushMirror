using UnityEngine;

public class Ghoul : EnemyBase
{
    private enum GhoulState
    {
        Scanning,
        Charging
    }
    
    [SerializeField] private float detectionDistance = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float universalAgroDistance = 2f;
    
    private bool _isJumping;
    private GhoulState _currentState = GhoulState.Scanning;
    
    private new void Start()
    {
        base.Start();
        
        Initialize();
        
        _currentState = GhoulState.Scanning;
    }
    
    private new void Update()
    {
        base.Update();
        
        if (isDead)
        {
            targetVelocity = Vector2.zero;
            velocity = Vector2.zero;
            return;
        }

        switch (_currentState)
        {
            case GhoulState.Scanning:
                Vector2 playerDirection = CheckForPlayer();
                if (playerDirection != Vector2.zero)
                {
                    facingDirection = playerDirection;
                    _currentState = GhoulState.Charging;
                }
                break;
            
            case GhoulState.Charging:
                Charge();
                break;
        }
    }
    
    private void Charge()
    {
        if (!isKnockback)
        {
            targetVelocity = facingDirection * moveSpeed;
        }

        // Detect if the Ghoul charges into a wall
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, 1.5f, EnemyAndGroundLayerMask);
        if (hit.collider != null)
        {
            // If the Ghoul charges into a wall, stop charging and start scanning
            targetVelocity = Vector2.zero;
                
            _currentState = GhoulState.Scanning;
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
    
    private Vector2 CheckForPlayer()
    {
        // Check for a player in close proximity
        Collider2D hitClose = Physics2D.OverlapCircle(transform.position, universalAgroDistance, LayerMask.GetMask("Player"));
        if (hitClose != null)
        {
            // Return the direction to the player
            return (hitClose.transform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
        }

        // Cast a ray to the right
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, detectionDistance, LayerMask.GetMask("Player"));
        if (hitRight.collider != null)
        {
            // Return the direction to the player
            return Vector2.right;
        }

        // Cast a ray to the left
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, detectionDistance, LayerMask.GetMask("Player"));
        if (hitLeft.collider != null)
        {
            // Return the direction to the player
            return Vector2.left;
        }

        // If no player is detected, return a zero vector
        return Vector2.zero;
    }
}
