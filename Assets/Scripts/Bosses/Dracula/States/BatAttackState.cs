using System;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

public class BatAttackState : DraculaState
{
    [SerializeField] private float attackLength = 0.5f;
    [SerializeField] private float batSpawnRate = 0.1f;
    [SerializeField] private GameObject batPrefab;
    
    private float _timeSinceAttackStart;
    private float _timeSinceLastBatSpawn;
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
        _batPooler = TryGetCommunityPooler.TryGetPooler(batPrefab);
    }

    private void OnEnable()
    {
        var rightWall = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomRight);
        var leftWall = _cornerFinder.GetCorner(CornerFinder.CornerType.BottomLeft);
        
        var randomX = Random.Range(leftWall.x + 1, rightWall.x - 1);
        _teleportLocation = new Vector2(randomX, rightWall.y + Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y));
    }
    
    public void Update()
    {
        switch (_attackPhase)
        {
            case AttackPhase.Teleporting:
                if(!teleport.IsTeleporting) teleport.TeleportToLocation(_teleportLocation);
                break;
            case AttackPhase.SpawningBats:
                Bats();
                break;
        }
    }
    
    protected override void OnTeleportEnd()
    {
        _attackPhase = AttackPhase.SpawningBats;
        _timeSinceAttackStart = Time.time;
        _attackDirection = (_player.position - transform.position).normalized.x > 0 ? Vector2.right : Vector2.left;
    }

    private void Bats()
    {
        // If the attack has been going on for 5 seconds, switch to the next state
        if (Time.time - _timeSinceAttackStart > attackLength)
        {
            EndState();
            return;
        }
        
        // Spawn a bat every 0.5 seconds randomly on the attack side of the collider
        if (Time.time - _timeSinceLastBatSpawn > batSpawnRate)
        {
            _timeSinceLastBatSpawn = Time.time;
            var position = _attackDirection == Vector2.left ? _collider.bounds.min : _collider.bounds.max;
            position.y += Random.Range(0, _collider.bounds.size.y);
            var bat = _batPooler.GetPooledObject();
            bat.transform.position = position;
            bat.GetComponent<DraculaBatScript>().targetVector = _attackDirection;
            bat.SetActive(true);
        }
    }
}
