using UnityEngine;
using System.Collections;

public class BasketballHand : AUBHandState
{
    [SerializeField] private AnimationClip slamAnimation;
    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField] private float verticalSpeed = 5f;
    [SerializeField] private float bounceHeight = 5f;
    [SerializeField] private float endDistanceFromWall = 4f;

    private float _groundY;
    private float _oppositeWallX;
    private bool _isMovingUp = true;

    private void Awake()
    {
        RaycastHit2D hit = Physics2D.Raycast(Boss.bossCollider.bounds.center, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground"));
        if (hit.collider == null)
        {
            Debug.LogError("HandSlide: No ground found");
            return;
        }
        
        _groundY = hit.point.y; // Store the Y position of the ground
        
        // Calculate the X position of the opposite wall
        RaycastHit2D wallHit = Physics2D.Raycast(Boss.bossCollider.bounds.center, Vector2.right * transform.localScale.x, Mathf.Infinity, LayerMask.GetMask("Ground"));
        if (wallHit.collider == null)
        {
            Debug.LogError("HandSlide: No wall found");
            return;
        }
        
        _oppositeWallX = wallHit.point.x; // Store the X position of the opposite wall
    }
    
    private void OnEnable()
    {
        StartCoroutine(Slam());
    }

    private IEnumerator Slam()
    {
        Boss.AnimancerComponent.Play(slamAnimation);
        while (!Boss.bossCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            Boss.Move(Vector2.down * verticalSpeed);
            yield return null;
        }

        StartCoroutine(Basketball());
    }

    private IEnumerator Basketball()
    {
        while (true)
        {
            var horizontalVelocity = Vector2.left * (transform.localScale.x * horizontalSpeed);
            Vector2 verticalVelocity;
            if (_isMovingUp)
            {
                verticalVelocity = Vector2.up * verticalSpeed;
                if (transform.position.y >= _groundY + bounceHeight)
                {
                    _isMovingUp = false;
                    if (Boss.bossCollider.bounds.center.x * -transform.localScale.x + endDistanceFromWall >= _oppositeWallX * -transform.localScale.x)
                    {
                        break;
                    }
                }
            }
            else
            {
                verticalVelocity = Vector2.down * verticalSpeed;
                if (Boss.bossCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
                {
                    _isMovingUp = true;
                }
            }
            Boss.Move(horizontalVelocity + verticalVelocity);
            yield return null;
        }
        
        Boss.StateMachine.ForceSetState(hoverHand);
    }
}