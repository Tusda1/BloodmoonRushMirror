using System;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

public class BatAttackState : DraculaState
{
    [SerializeField] private AnimationClip batAttackStart;
    [SerializeField] private AnimationClip batAttack;
    [SerializeField] private float attackLength = 0.5f;
    [SerializeField] private float batSpawnRate = 0.1f;
    [SerializeField] private GameObject batPrefab;

    private bool _batAnimationPlayed;
    private bool _batAttackStarted;
    private float _attackTimer;
    private float _batTimer;
    private bool _startPlayed;
    private Vector2 _teleportLocation;
    private Transform _player;
    private Vector2 _attackDirection;
    
    private Collider2D _collider;
    private Pooler _batPooler;
    private CornerFinder _cornerFinder;
    
    private enum AttackPhase
    {
        Teleporting,
        SpawningBats
    }
    
    private AttackPhase _attackPhase;

    protected override void Awake()
    {
        base.Awake();
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
        _collider = GetComponent<Collider2D>();
    }
    
    private void Start()
    {
        _player = ServiceLocator.Get<PlayerScript>().transform;
        _batPooler = TryGetCommunityPooler.TryGetPooler(batPrefab, ObjectResolver);
    }

    private void OnEnable()
    {
        var rightWall = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        var leftWall = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);

        var tries = 0;
        var safePosition = false;
        var setPosition = Vector2.zero;
        while (!safePosition)
        {
            tries++;
            
            if (tries > 100)
            {
                Debug.LogError("Could not find a safe position to teleport to");
                break;
            }
            
            var randomX = Random.Range(leftWall.x + 1, rightWall.x - 1);
            setPosition = new Vector2(randomX, rightWall.y);
            
            // Use a boxcast to determine that you are not teleporting on top of the player
            var hit = Physics2D.BoxCast(setPosition, _collider.bounds.size, 0, Vector2.zero, 0, LayerMask.GetMask("Player"));
            safePosition = hit.collider == null;
        }

        _teleportLocation = setPosition;
        
        _attackPhase = AttackPhase.Teleporting;
    }

    public void Update()
    {
        switch (_attackPhase)
        {
            case AttackPhase.Teleporting:
                if (!teleport.IsTeleporting)
                {
                    var teleportEnter = Boss.isGrounded ? teleportEnterGround : teleportEnterAir;
                    teleport.TeleportToLocation(_teleportLocation, teleportEnter, teleportExit);
                }
                break;
            case AttackPhase.SpawningBats:
                Bats();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _attackPhase = AttackPhase.SpawningBats;
        _attackDirection = (_player.position - transform.position).normalized.x > 0 ? Vector2.right : Vector2.left;
    }

    private void Bats()
    {
        if (!_batAttackStarted)
        {
            Boss.facingDirection = _attackDirection;
            var state = Boss.AnimancerComponent.Play(batAttackStart);
            state.Events(this).OnEnd = () =>
            {
                Boss.AnimancerComponent.Play(batAttack);
                _batAnimationPlayed = true;
                state.Destroy();
            };
            _batAttackStarted = true;
        }
        
        if (!_batAnimationPlayed) return;
        
        // If the attack has been going on for 5 seconds, switch to the next state
        _attackTimer += Time.deltaTime;
        if (_attackTimer > attackLength)
        {
            _attackTimer = 0;
            _batAnimationPlayed = false;
            _batAttackStarted = false;
            EndState();
            return;
        }
        
        // Spawn a bat every 0.5 seconds randomly on the attack side of the collider
        _batTimer += Time.deltaTime;
        if (_batTimer > batSpawnRate)
        {
            _batTimer = 0;
            var bounds = _collider.bounds;
            var draculaPosition = transform.position;
            var position = Vector3.zero;
            position.x = _attackDirection == Vector2.left ? draculaPosition.x - 0.2f : draculaPosition.x + 0.2f;
            position.y = Random.Range(bounds.min.y + 0.5f, bounds.max.y - 0.5f);
            var bat = _batPooler.GetPooledObject();
            bat.transform.position = position;
            bat.GetComponent<DraculaBatScript>().targetVector = _attackDirection;
            bat.SetActive(true);
        }
    }
}
