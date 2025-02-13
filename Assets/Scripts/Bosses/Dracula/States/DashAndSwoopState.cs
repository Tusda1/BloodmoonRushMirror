using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class DashAndSwoopState : DraculaState
{
    private Vector2 _bottomRight;
    private Vector2 _bottomLeft;
    private Vector2 _topRight;
    private Vector2 _topLeft;
    private Vector2 _bottomMiddle;
    private Vector2 _teleportLocation;
    
    [SerializeField] private int loopAmount = 5;
    private int _loopCount;

    [SerializeField] private AnimationClip dashTeleportEnter;
    [SerializeField] private AnimationClip dashTeleportExit;
    [SerializeField] private AnimationClip dashAnimation;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashProjectileCooldown = 1f;
    [SerializeField] private int dashProjectilesPerVolley = 3;
    private Vector2 _dashTarget;
    private bool _dashStarted;
    
    [SerializeField] private AnimationClip swoopTeleportEnter;
    [SerializeField] private AnimationClip swoopTeleportExit;
    [SerializeField] private float swoopProjectileCooldown = 1f;
    [SerializeField] private int swoopProjectilesPerVolley = 3;
    private bool _swoopStarted;
    private Vector2 _swoopOrigin;
    private Vector2 _swoopTarget;
    
    private float _shootTimer;
    
    [SerializeField] private Swoop swoop = new();
    private PlayerScript _player;
    private GameObject _homingProjectile;
    private CornerFinder _cornerFinder;
    private Pooler _projectilePooler;
    
    private enum Stage
    {
        Dashing,
        Swooping
    }
    
    private List<Stage> _stages = new List<Stage>();

    protected override void Awake()
    {
        base.Awake();
        swoop.InitializeSwoop(SwoopEnd, Boss);
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
        _homingProjectile = Resources.Load<GameObject>("Prefabs/HomingProjectile");
    }

    private void Start()
    {
        _player = ServiceLocator.Get<PlayerScript>();
        _projectilePooler = TryGetCommunityPooler.TryGetPooler(_homingProjectile);
    }

    private void OnEnable()
    {
        _topLeft = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
        _topLeft.x += 1f;
        _topLeft.y -= 2f;
        _topRight = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);
        _topRight.x -= 1f;
        _topRight.y -= 2f;
        _bottomRight = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        _bottomRight.x -= 1f;
        _bottomLeft = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        _bottomLeft.x += 1f;
        
        _bottomMiddle = (_bottomRight + _bottomLeft) / 2;
        
        _stages.Add(default);
        for (int i = 0; i < loopAmount; i++)
        {
            // Add random stages to the list
            _stages.Add(UnityEngine.Random.value > 0.5f ? Stage.Dashing : Stage.Swooping);
        }
        
        if (_stages[_loopCount] == Stage.Dashing)
        {
            _teleportLocation = UnityEngine.Random.value > 0.5f ? _bottomRight : _bottomLeft;
        }
        else
        {
            _teleportLocation = UnityEngine.Random.value > 0.5f ? _topRight : _topLeft;
        }
        
        TeleportTrigger();
    }

    private void OnDisable()
    {
        _shootTimer = 0;
        Boss.transform.rotation = Quaternion.identity;
    }

    private void MoveToNextLoop()
    {
        if (_stages[_loopCount] != _stages[_loopCount - 1])
        {
            if (_stages[_loopCount] == Stage.Dashing)
            {
                _teleportLocation = UnityEngine.Random.value > 0.5f ? _bottomRight : _bottomLeft;
            }
            else
            {
                _teleportLocation = UnityEngine.Random.value > 0.5f ? _topRight : _topLeft;
            }
            
            TeleportTrigger();
        }
        else
        {
            if (_stages[_loopCount] == Stage.Dashing)
            {
                _teleportLocation = _teleportLocation == _bottomRight ? _bottomLeft : _bottomRight;
            }
            else
            {
                _teleportLocation = _teleportLocation == _topRight ? _topLeft : _topRight;
            }
            
            _loopCount++;
        }
    }

    private void TeleportTrigger()
    {
        AnimationClip currentTeleportEnter = null;
        AnimationClip currentTeleportExit = null;

        if (_loopCount == 0)
        {
            currentTeleportEnter = Boss.isGrounded ? teleportEnterGround : teleportEnterAir;
            currentTeleportExit = _stages[0] == Stage.Dashing ? dashTeleportExit : swoopTeleportExit;
        }
        else
        {
            currentTeleportEnter = _stages[_loopCount] == Stage.Dashing ? dashTeleportEnter : swoopTeleportEnter;
            currentTeleportExit = _stages[_loopCount] == Stage.Dashing ? dashTeleportExit : swoopTeleportExit;
        }

        teleport.TeleportToLocation(_teleportLocation, currentTeleportEnter, currentTeleportExit, () => _loopCount++);
    }

    private void Dash()
    {
        if (!_dashStarted)
        {
            _dashStarted = true;
            Boss.EnableGravity();
            Boss.AnimancerComponent.Play(dashAnimation);
            
            _dashTarget = _teleportLocation == _bottomRight ? _bottomLeft : _bottomRight;
        }
        
        if (!Boss.bossGrounded) return;
        var direction = _dashTarget == _bottomRight ? Vector2.right : Vector2.left;
        Boss.targetVelocity = direction * dashSpeed;
        // Check if the boss crossed the finish line
        if (transform.position.x * direction.x > _dashTarget.x * direction.x)
        {
            MoveToNextLoop();
            Boss.DisableGravity();
            _dashStarted = false;
        }
        else
        {
            if (_shootTimer > 0)
            {
                _shootTimer -= Time.deltaTime;
            }
            else
            {
                StartCoroutine(Shoot(dashProjectilesPerVolley, Vector2.up));
                _shootTimer = dashProjectileCooldown;
            }
        }
    }
    
    private void SwoopStage()
    {
        if (!_swoopStarted)
        {
            if (_teleportLocation != _swoopOrigin)
            {
                (_swoopOrigin, _swoopTarget) = (_swoopTarget, _swoopOrigin);
            }
            
            _swoopStarted = true;
        }

        var oldPos = transform.position;
        swoop.ExecuteSwoop(_swoopOrigin, _swoopTarget, _bottomMiddle, 0.9f);

        if (_shootTimer > 0)
        {
            _shootTimer -= Time.deltaTime;
        }
        else
        {
            // Find the normal on the vector between the current position and the new position
            var directionVector = (transform.position - oldPos).normalized;
            var up = new Vector2(directionVector.y, -directionVector.x);
            StartCoroutine(Shoot(swoopProjectilesPerVolley, up));
            _shootTimer = swoopProjectileCooldown;
        }
    }
    
    private void SwoopEnd()
    {
        MoveToNextLoop();
        _swoopStarted = false;
    }

    private IEnumerator Shoot(int projectilesInVolley, Vector2 initialDirection)
    {
        for (int i = 0; i < projectilesInVolley; i++)
        {
            var projectile = _projectilePooler.GetPooledObject();
            projectile.transform.position = transform.position;
            var script = projectile.GetComponent<HomingProjectile>();
            script.target = _player.gameObject;
            script.initialDirection = initialDirection;
            projectile.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
