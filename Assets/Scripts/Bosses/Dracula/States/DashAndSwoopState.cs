using System;
using System.Collections;
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
    private bool _isFirstLoop;
    private AnimationClip _currentTeleportEnter;
    private AnimationClip _currentTeleportExit;

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
        Brain,
        Teleporting,
        Dashing,
        Swooping
    }
    
    private Stage _stage;
    private Stage _currentStage;

    protected override void Awake()
    {
        base.Awake();
        swoop.InitializeSwoop(SwoopEnd, Boss.AnimancerComponent, Boss);
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
        _bottomRight.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        _bottomLeft = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        _bottomLeft.x += 1f;
        _bottomLeft.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        
        _bottomMiddle = (_bottomRight + _bottomLeft) / 2;
        
        _isFirstLoop = true;
        
        _currentStage = Stage.Brain;
    }

    private void OnDisable()
    {
        _shootTimer = 0;
    }

    private void Update()
    {
        switch (_currentStage)
        {
            case Stage.Brain:
                Calculate();
                break;
            case Stage.Teleporting:
                if (!teleport.IsTeleporting) teleport.TeleportToLocation(_teleportLocation, _currentTeleportEnter, _currentTeleportExit);
                break;
            case Stage.Dashing:
                Dash();
                break;
            case Stage.Swooping:
                SwoopStage();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _currentStage = _stage;
    }

    private void Calculate()
    {
        
        if (_loopCount < loopAmount)
        {
            _loopCount++;
        }
        else
        {
            _loopCount = 0;
            EndState();
            return;
        }
        
        // 50% chance to do a dash or a swoop
        if (UnityEngine.Random.value > 0.5f)
        {
            _stage = Stage.Dashing;
            // 50% chance to teleport left or right
            _teleportLocation = UnityEngine.Random.value > 0.5f ? _bottomRight : _bottomLeft;
        }
        else
        {
            _stage = Stage.Swooping;
            // 50% chance to teleport left or right
            _teleportLocation = UnityEngine.Random.value > 0.5f ? _topRight : _topLeft;
        }

        if (_isFirstLoop)
        {
            _isFirstLoop = false;
            _currentTeleportEnter = Boss.isGrounded ? teleportEnterGround : teleportEnterAir;
            _currentTeleportExit = _stage == Stage.Dashing ? dashTeleportExit : swoopTeleportExit;
        }
        else
        {
            _currentTeleportEnter = _stage == Stage.Dashing ? dashTeleportEnter : swoopTeleportEnter;
            _currentTeleportExit = _stage == Stage.Dashing ? dashTeleportExit : swoopTeleportExit;
        }
        
        _currentStage = Stage.Teleporting;
    }
    
    private void Dash()
    {
        if (!_dashStarted)
        {
            _dashStarted = true;
            Boss.EnableGravity();
            _dashTarget = _teleportLocation == _bottomRight ? _bottomLeft : _bottomRight;
            Boss.AnimancerComponent.Play(dashAnimation);
        }
        
        if (!Boss.bossGrounded) return;
        
        transform.position = Vector2.MoveTowards(transform.position, _dashTarget, dashSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, _dashTarget) < 0.1f)
        {
            _currentStage = Stage.Brain;
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
            if (_teleportLocation == _topRight)
            {
                _swoopOrigin = _topRight;
                _swoopTarget = _topLeft;
            }
            else
            {
                _swoopOrigin = _topLeft;
                _swoopTarget = _topRight;
            }
            
            _swoopStarted = true;
        }

        var oldPos = transform.position;
        swoop.ExecuteSwoop(_swoopOrigin, _swoopTarget, _bottomMiddle);

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
        _currentStage = Stage.Brain;
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
