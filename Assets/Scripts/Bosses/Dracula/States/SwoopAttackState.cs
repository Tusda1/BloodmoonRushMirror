using System;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class SwoopAttackState : DraculaState
{
    [SerializeField] private float shootCooldown = 0.5f;
    [SerializeField] private float shootDuration = 1f;
    [SerializeField] private int swoopAmount = 3;
    
    private Vector2 _swoopOrigin;
    private Vector2 _swoopTarget;
    private Vector2 _swoopArcBottom;
    private int swoopCount;
    private SwoopState _swoopState;
    private float _shootTimer;
    private float _timeSinceShoot;
    private Vector2 _temporaryPosition;
    
    [SerializeField] private Swoop swoop = new();
    private Transform _player;
    
    private enum SwoopState 
    {
        Teleporting,
        Swooping,
        Shooting,
        Finished
    }

    private void Start()
    {
        swoop.SetOnSwoopEnd(SwoopEnd);
        _player = ServiceLocator.Get<PlayerScript>().transform;
    }

    private void OnEnable()
    {
        swoopCount = 0;
        
        // Send a ray straight up and find the roof
        var hit = Physics2D.Raycast(transform.position, Vector2.up, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find roof"); return;}
        
        var roof = hit.point;
        roof.y -= 0.1f;
        
        // Find the right corner
        hit = Physics2D.Raycast(roof, Vector2.right, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find right corner"); return;}
        
        Vector2 rightCorner = hit.point;
        rightCorner.x -= 1f;
        rightCorner.y -= 2f;
        
        // Find the left corner
        hit = Physics2D.Raycast(roof, Vector2.left, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find left corner"); return;}
        
        Vector2 leftCorner = hit.point;
        leftCorner.x += 1f;
        leftCorner.y -= 2f;
        
        // 50% chance to go left or right
        if (UnityEngine.Random.value > 0.5f)
        {
            _swoopOrigin = rightCorner;
            _swoopTarget = leftCorner;
        }
        else
        {
            _swoopOrigin = leftCorner;
            _swoopTarget = rightCorner;
        }
        
        // Send a ray from exactly between the two corners to find the arc bottom
        _swoopArcBottom = (rightCorner + leftCorner) / 2;
        hit = Physics2D.Raycast(_swoopArcBottom, Vector2.down, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find arc bottom"); return;}
        
        _swoopArcBottom = hit.point + Vector2.up;
        
        _swoopState = SwoopState.Teleporting;
    }

    private void Update()
    {
        switch (_swoopState)
        {
            case SwoopState.Teleporting:
                if (!teleport.IsTeleporting) teleport.TeleportToLocation(_swoopOrigin);
                break;
            case SwoopState.Swooping:
                transform.position = swoop.ExecuteSwoop(_swoopOrigin, _swoopTarget, _swoopArcBottom);
                break;
            case SwoopState.Shooting:
                Shoot();
                break;
            case SwoopState.Finished:
                EndState();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _swoopState = SwoopState.Swooping;
    }

    private void SwoopEnd()
    {
        _swoopState = SwoopState.Shooting;
        swoopCount++;
        _timeSinceShoot = Time.time;
        (_swoopOrigin, _swoopTarget) = (_swoopTarget, _swoopOrigin);
    }
    
    private void Shoot()
    {
        // If the shoot duration has passed, switch to the next swoop
        if (Time.time - _timeSinceShoot > shootDuration)
        {
            if (swoopCount >= swoopAmount)
            {
                EndState();
            }
            else
            {
                _swoopState = SwoopState.Swooping;
            }
        }
        // Shoot a projectile at the player
        else if (Time.time - _shootTimer > shootCooldown)
        {
            _shootTimer = Time.time;
            
            // Find a random point in a radius around the player
            Vector2 randomPoint = (Vector2)_player.position + UnityEngine.Random.insideUnitCircle * 2;
            
            // Shoot a projectile at the random point
            var position = transform.position;
            var projectile = DraculaReference.Pooler.GetPooledObject();
            projectile.transform.position = position;
            projectile.GetComponent<DraculaBatScript>().targetVector = (randomPoint - (Vector2)position).normalized;
            projectile.SetActive(true);
        }
    }
}
