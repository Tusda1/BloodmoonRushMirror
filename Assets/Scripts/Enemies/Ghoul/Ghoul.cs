using UnityEngine;

public class Ghoul : ReactiveBase
{
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float universalAgroDistance = 2f;
    
    private bool _isJumping;
    
    private GhoulAnimations _animations;

    protected new void Awake()
    {
        base.Awake();
        _animations = GetComponent<GhoulAnimations>();
    }
    
    protected override void Scan()
    {
        // Check for a player in close proximity
        Collider2D hitClose = Physics2D.OverlapCircle(enemyCollider.bounds.center, universalAgroDistance, LayerMask.GetMask("Player"));
        if (hitClose != null)
        {
            // Return the direction to the player
            facingDirection = (hitClose.transform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
            SwapToAction();
            return;
        }

        // Cast a ray to the right
        RaycastHit2D hitRight = Physics2D.Raycast(enemyCollider.bounds.center, Vector2.right, detectionDistance, LayerMask.GetMask("Player", "Ground"));
        if (hitRight && hitRight.collider.CompareTag("Player"))
        {
            // Return the direction to the player
            facingDirection = Vector2.right;
            SwapToAction();
            return;
        }

        // Cast a ray to the left
        RaycastHit2D hitLeft = Physics2D.Raycast(enemyCollider.bounds.center, Vector2.left, detectionDistance, LayerMask.GetMask("Player", "Ground"));
        if (hitLeft && hitLeft.collider.CompareTag("Player"))
        {
            // Return the direction to the player
            facingDirection = Vector2.left;
            SwapToAction();
        }
    }

    private void SwapToAction()
    {
        currentState = EnemyState.Action;
        _animations.PlayRunAnimation();
    }

    protected override void Action()
    {
        targetVelocity = facingDirection * moveSpeed;
        
        // Detect if the Ghoul charges into a wall
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, 1f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            // If the Ghoul charges into a wall, stop charging and start scanning
            targetVelocity = Vector2.zero;
            currentState = EnemyState.Scanning;
            _animations.PlayDefaultAnimation();
            return;
        }
            
        if (!_isJumping)
        {
            hit = Physics2D.Raycast(GetBottomPosition() + facingDirection, Vector2.down, 1f, LayerMask.GetMask("Ground"));
            if (hit.collider == null)
            {
                // If the ghoul is about to fall off a ledge, jump off the ledge
                velocity.y = jumpForce;
                _isJumping = true;
                _animations.PlayJumpAnimation();
            }
        }
        else if (isGrounded)
        {
            _isJumping = false;
            _animations.PlayRunAnimation();
        }
    }
}
