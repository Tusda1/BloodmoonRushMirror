using UnityEngine;

public class JumpEnemy : ReactiveBase
{
    [SerializeField] private float rayDistance = 1f;
    [SerializeField] private float keepAgroDistance = 5f;
    
    private Transform _playerTransform;
    private Rigidbody2D _playerRb2D;
    private bool isJumping;
    
    protected override void Scan()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, LayerMask.GetMask("Player"));
        if (hit.collider != null)
        {
            currentState = EnemyState.Action;
            _playerRb2D = hit.collider.attachedRigidbody;
            _playerTransform = hit.collider.transform;
        }
    }
    
    protected override void Action()
    {
        if (_playerTransform == null && Vector2.Distance(_playerTransform.position, transform.position) > keepAgroDistance)
        {
            currentState = EnemyState.Scanning;
            return;
        }
        
        if (_playerRb2D.linearVelocityY > 0 && !isJumping)
        {
            Jump();
        }
        else if (isGrounded)
        {
            isJumping = false;
        }
    }
    
    private void Jump()
    {
        velocity.y = _playerRb2D.linearVelocityY;
        isJumping = true;
    }
}
