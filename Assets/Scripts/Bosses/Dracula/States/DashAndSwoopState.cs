using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using Player;
using Unity.VisualScripting;
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
    
    [SerializeField] private AnimationClip dashTeleportEnter;
    [SerializeField] private AnimationClip dashTeleportExit;
    [SerializeField] private AnimationClip dashAnimation;
    [SerializeField] private AnimationClip swoopTeleportEnter;
    [SerializeField] private AnimationClip swoopTeleportExit;
    [SerializeField] private AnimationClip airIdleAnimation;
    
    [SerializeField] private int loopAmount = 5;
    private int _loopCount;
    
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDestinationOffset = 1f;
    private Vector2 _dashTarget;
    private bool _dashStarted;
    
    [SerializeField] private int volleyAmount = 3;
    [SerializeField] private float volleyCooldown = 0.05f;
    [SerializeField] private float volleyDelay = 0.2f;
    [SerializeField] private int swoopProjectilesPerVolley = 3;
    [SerializeField] private float swoopCompletionLevel = 0.9f;
    private bool _swoopStarted;
    private Vector2 _swoopOrigin;
    private Vector2 _swoopTarget;
    
    [SerializeField] private Swoop swoop = new();
    private PlayerScript _player;
    private GameObject _homingProjectile;
    private CornerFinder _cornerFinder;
    private Pooler _projectilePooler;
    
    private enum Stage
    {
        Starting,
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
        _projectilePooler = TryGetCommunityPooler.TryGetPooler(_homingProjectile, ObjectResolver);
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
        _bottomRight.x -= 1.5f;
        _bottomLeft = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        _bottomLeft.x += 1.5f;
        
        _bottomMiddle = (_bottomRight + _bottomLeft) / 2;
        _bottomMiddle.y += 1.5f;
        
        _stages.Add(Stage.Starting);
        for (int i = 0; i < loopAmount; i++)
        {
            // Add random stages to the list
            _stages.Add(UnityEngine.Random.value > 0.5f ? Stage.Dashing : Stage.Swooping);
        }
        
        // Random teleport location
        if (_stages[_loopCount + 1] == Stage.Dashing)
        {
            _teleportLocation = UnityEngine.Random.value > 0.5f ? _bottomRight : _bottomLeft;
        }
        else
        {
            _teleportLocation = UnityEngine.Random.value > 0.5f ? _topRight : _topLeft;
        }
        
        var teleportEnter = Boss.isGrounded ? teleportEnterGround : teleportEnterAir;
        var tempTeleportExit = _stages[_loopCount + 1] == Stage.Dashing ? dashTeleportExit : swoopTeleportExit;
        teleport.TeleportToLocation(_teleportLocation, teleportEnter, tempTeleportExit, MidTeleport);
    }

    private void OnDisable()
    {
        Boss.transform.rotation = Quaternion.identity;
        swoop.ResetSwoop();
    }

    private void Update()
    {
        switch (_stages[_loopCount])
        {
            case Stage.Starting:
                break;
            case Stage.Dashing:
                Dash();
                break;
            case Stage.Swooping:
                SwoopStage();
                break;
        }
    }

    private void StageTransition()
    {
        if (teleport.IsTeleporting) return;
        
        if (_loopCount >= _stages.Count - 1)
        {
            EndState();
            return;
        }

        // Random teleport location
        if (_stages[_loopCount + 1] == Stage.Dashing)
        {
            _teleportLocation = UnityEngine.Random.value > 0.5f ? _bottomRight : _bottomLeft;
        }
        else
        {
            _teleportLocation = UnityEngine.Random.value > 0.5f ? _topRight : _topLeft;
        }
        
        var tempTeleportEnter = _stages[_loopCount] == Stage.Dashing ? dashTeleportEnter : swoopTeleportEnter;
        var tempTeleportExit = _stages[_loopCount + 1] == Stage.Dashing ? dashTeleportExit : swoopTeleportExit;
        
        teleport.TeleportToLocation(_teleportLocation, tempTeleportEnter, tempTeleportExit, MidTeleport);
    }
    
    private void MidTeleport()
    {
        _loopCount++;
        Boss.DisableGravity();
        _dashStarted = false;
        _swoopStarted = false;
        Boss.transform.rotation = Quaternion.identity;
        swoop.ResetSwoop();
    }

    protected override void OnTeleportEnd()
    {
        if (_stages[_loopCount] == Stage.Dashing)
        {
            Boss.AnimancerComponent.Play(dashAnimation);
        }
    }

    private void Dash()
    {
        if (!_dashStarted)
        {
            _dashStarted = true;
            Boss.EnableGravity();
            
            _dashTarget = _teleportLocation == _bottomRight ? _bottomLeft : _bottomRight;
        }
        
        if (!Boss.bossGrounded) return;
        var direction = _dashTarget == _bottomRight ? Vector2.right : Vector2.left;
        Boss.targetVelocity = direction * dashSpeed;
        // Check if the boss crossed the finish line
        if (transform.position.x * direction.x + dashDestinationOffset * direction.x * Mathf.Sign(transform.position.x) > _dashTarget.x * direction.x)
        {
            if (_loopCount != _stages.Count - 1)
            {
                StageTransition();  
            }
            else
            {
                StartCoroutine(WaitBeforeNextState());
            }
        }
    }
    
    private void SwoopStage()
    {
        if (!_swoopStarted)
        {
            _swoopOrigin = _teleportLocation;
            _swoopTarget = _swoopOrigin == _topRight ? _topLeft : _topRight; 
            
            _swoopStarted = true;

            StartCoroutine(Shoot());
        }

        var oldPos = transform.position;
        
        if (_loopCount != _stages.Count - 1)
        {
            swoop.ExecuteSwoop(_swoopOrigin, _swoopTarget, _bottomMiddle, !teleport.IsTeleporting, swoopCompletionLevel);
        }
        else
        { 
            swoop.ExecuteSwoop(_swoopOrigin, _swoopTarget, _bottomMiddle, !teleport.IsTeleporting);
        }
    }
    
    private void SwoopEnd()
    {
        if (_loopCount != _stages.Count - 1)
        {
            StageTransition();
        }
        else
        {
            StartCoroutine(WaitBeforeNextState());
        }
    }
    
    private IEnumerator WaitBeforeNextState()
    {
        Boss.transform.rotation = Quaternion.identity;

        Boss.AnimancerComponent.Play(Boss.isGrounded ? idleAnimation : airIdleAnimation);

        yield return new WaitForSeconds(1f);
        EndState();
    }

    private IEnumerator Shoot()
    {
        var timeNeededPerVolley = swoopProjectilesPerVolley * volleyCooldown;
        var timeTotal = swoop.swoopDuration - volleyDelay * 2;
        var timeLeftOver = timeTotal - timeNeededPerVolley * volleyAmount;
        var timeBetweenVolleys = timeLeftOver / (volleyAmount - 1);
        
        yield return new WaitUntil(() => swoop.t * swoop.swoopDuration > volleyDelay);

        yield return StartCoroutine(FireVolley());
        
        for (int i = 0; i < volleyAmount - 1; i++)
        {
            yield return new WaitForSeconds(timeBetweenVolleys);
            yield return StartCoroutine(FireVolley());
        }
    }
    
    private IEnumerator FireVolley()
    {
        for (int i = 0; i < swoopProjectilesPerVolley; i++)
        {
            var projectile = _projectilePooler.GetPooledObject();
            projectile.transform.position = transform.position;
            var script = projectile.GetComponent<HomingProjectile>();
            script.target = _player.gameObject;
            script.initialDirection = Vector2.up;
            projectile.SetActive(true);
            yield return new WaitForSeconds(volleyCooldown);
        }
    }
}
