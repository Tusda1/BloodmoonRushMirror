using System;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

public class DashStrike : DrucillaState
{
    [SerializeField] private AnimationClip lightningWindupStart;
    [SerializeField] private AnimationClip lightningWindup;
    [SerializeField] private AnimationClip lightningEnd;
    [SerializeField] private AnimationClip strikeAnimation;
    
    [SerializeField] private Collider2D strikeHitBox;
    [SerializeField] private Vector2 dashDestinationOffset;
    [SerializeField] private Vector2 dashOriginOffset;
    [SerializeField] private float tooCloseDistance = 4f;
    [SerializeField] private DamageInstance damageInstance;
    
    private Vector2 _teleportLocation;
    private Vector2 _dashDestination;
    private bool _lightningDashStarted;
    
    [SerializeField] private LightningManager lightningManager = new();
    private PlayerScript _player;
    private CornerFinder _cornerFinder;
    
    private enum DashStrikeStage
    {
        Teleport,
        LightningDash
    }
    
    private DashStrikeStage _stage;
    
    protected override void Awake()
    {
        base.Awake();
        
        _cornerFinder = new CornerFinder(gameObject.transform, LayerMask.GetMask("Ground"));
    }

    private void Start()
    {
        lightningManager.InitializeLightningManager(ObjectResolver);
    }

    private void OnEnable()
    {
        if (!_player) _player = ServiceLocator.Get<PlayerScript>();

        if (Vector2.Distance(transform.position, _player.transform.position) < tooCloseDistance)
        {
            var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
            var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);

            if (Vector2.Distance(leftCorner, _player.transform.position) < Vector2.Distance(rightCorner, _player.transform.position))
            {
                leftCorner.x += 1;
                leftCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
                _teleportLocation = leftCorner;
            }
            else
            {
                rightCorner.x -= 1;
                rightCorner.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
                _teleportLocation = rightCorner;
            }
            
            _stage = DashStrikeStage.Teleport;
        }
        else
        {
            _stage = DashStrikeStage.LightningDash;
        }
    }

    private void OnDisable()
    {
        _lightningDashStarted = false;
    }

    private void Update()
    {
        switch (_stage)
        {
            case DashStrikeStage.Teleport:
                if (!teleport.IsTeleporting) teleport.TeleportToLocation(_teleportLocation, DrucillaReference.TeleportEnter, DrucillaReference.TeleportExit);
                break;
            case DashStrikeStage.LightningDash:
                LightningDash();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _stage = DashStrikeStage.LightningDash;
    }
    
    private void LightningDash()
    {
        if (!_lightningDashStarted)
        {
            _lightningDashStarted = true;
            var position = transform.position;
            var state = Boss.AnimancerComponent.Play(lightningWindupStart);
            state.Events(this).OnEnd = () =>
            {
                state.Destroy();
                state = Boss.AnimancerComponent.Play(lightningWindup);
                state.Events(this).OnEnd = () =>
                {
                    state.Destroy();
                    Boss.facingDirection = _player.transform.position.x > transform.position.x ? Vector2.right : Vector2.left;
                    _dashDestination = _player.transform.position + (Vector3)(dashDestinationOffset * new Vector2(Boss.facingDirection.x, 1));
                    lightningManager.CreateLightning(position + (Vector3)(dashOriginOffset * new Vector2(Boss.facingDirection.x, 1)), _dashDestination);
                    OnLightningEnd();
                };
            };
        }
    }
    
    private void OnLightningEnd()
    {
        transform.position = _dashDestination;
        var state = Boss.AnimancerComponent.Play(lightningEnd);
        state.Events(this).OnEnd = () =>
        {
            state.Destroy();
            Strike();
            var state2 = Boss.AnimancerComponent.Play(strikeAnimation);
            state2.Events(this).OnEnd = () =>
            {
                state2.Destroy();
                Boss.AnimancerComponent.Play(idleAnimation);
                EndState();
            };
        };
    }

    private void Strike()
    {
        // Use the confines of the box collider to cast an overlapbox to find the player
        var hit = Physics2D.OverlapBox(strikeHitBox.bounds.center, strikeHitBox.bounds.size, 0, LayerMask.GetMask("Player"));
        if (!hit) return;
        damageInstance.knockback = Boss.facingDirection;
        hit.GetComponent<IDamageable>().TakeDamage(damageInstance);
    }
}