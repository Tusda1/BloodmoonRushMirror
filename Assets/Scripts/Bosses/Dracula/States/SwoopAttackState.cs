using System;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class SwoopAttackState : DraculaState
{
    [SerializeField] private float shootCooldown = 0.5f;
    [SerializeField] private int shootAmount = 3;
    [SerializeField] private int swoopAmount = 3;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Vector2 projectileOriginOffset;
    
    private Vector2 _swoopOrigin;
    private Vector2 _swoopTarget;
    private Vector2 _swoopArcBottom;
    private int _swoopCount;
    private float _shootTimer;
    private float _shotCounter;
    private Vector2 _temporaryPosition;
    private Vector2 _lastFramePosition;
    
    [SerializeField] private Swoop swoop = new();
    private Transform _player;
    private Pooler _projectilePooler;
    private CornerFinder _cornerFinder;
    
    private enum SwoopState 
    {
        Teleporting,
        Swooping,
        Shooting
    }
    
    private SwoopState _swoopState;

    protected override void Awake()
    {
        base.Awake();
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
        swoop.InitializeSwoop(SwoopEnd, Boss);
    }

    private void Start()
    {
        _player = ServiceLocator.Get<PlayerScript>().transform;
        _projectilePooler = TryGetCommunityPooler.TryGetPooler(projectilePrefab, objectResolver);
    }

    private void OnEnable()
    {
        var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
        leftCorner.x += 1f;
        leftCorner.y -= 5f;
        var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);
        rightCorner.x -= 1f;
        rightCorner.y -= 5f;
        
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
        var hit = Physics2D.Raycast(_swoopArcBottom, Vector2.down, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find arc bottom"); return;}
        
        _swoopArcBottom = hit.point + Vector2.up;
        
        _swoopState = SwoopState.Teleporting;
    }

    private void OnDisable()
    {
        _shotCounter = 0;
        _swoopCount = 0;
    }

    private void Update()
    {
        switch (_swoopState)
        {
            case SwoopState.Teleporting:
                if (!teleport.IsTeleporting)
                {
                    var teleportEnter = Boss.isGrounded ? teleportEnterGround : teleportEnterAir;
                    teleport.TeleportToLocation(_swoopOrigin, teleportEnter, teleportExit);
                }
                break;
            case SwoopState.Swooping:
                swoop.ExecuteSwoop(_swoopOrigin, _swoopTarget, _swoopArcBottom);
                break;
            case SwoopState.Shooting:
                Shoot();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _swoopState = SwoopState.Swooping;
    }
    
    private void SwoopEnd()
    {
        swoop.ResetSwoop();
        Boss.transform.rotation = Quaternion.identity;
        _swoopState = SwoopState.Shooting;
        _swoopCount++;
        (_swoopOrigin, _swoopTarget) = (_swoopTarget, _swoopOrigin);
        Boss.AnimancerComponent.Play(idleAnimation);
    }
    
    private void Shoot()
    {
        // If the shoot duration has passed, switch to the next swoop
        if (_shotCounter >= shootAmount)
        {
            if (_swoopCount >= swoopAmount)
            {
                EndState();
            }
            else
            {
                _swoopState = SwoopState.Swooping;
                _shotCounter = 0;
            }
        }
        
        // Shoot a projectile at the player
        else if (Time.time - _shootTimer > shootCooldown)
        {
            _shootTimer = Time.time;
            _shotCounter++;
            
            // Find a random point in a radius around the player
            Vector2 randomPoint = (Vector2)_player.position + UnityEngine.Random.insideUnitCircle * 2;
            
            // Shoot a projectile at the random point
            var position = Boss.bossCollider.bounds.center;
            var projectile = _projectilePooler.GetPooledObject();
            projectile.transform.position = (Vector2)position + projectileOriginOffset;
            projectile.GetComponent<DraculaBatScript>().targetVector = (randomPoint - (Vector2)position).normalized;
            projectile.SetActive(true);
        }
    }
}
