using System;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

public class BatAttackState : DraculaState
{
    [SerializeField] private float attackLength = 0.5f;
    [SerializeField] private float batSpawnRate = 0.1f;
    
    private Collider2D _collider;
    private float _timeSinceAttackStart;
    private float _timeSinceLastBatSpawn;
    private Vector2 _teleportLocation;
    private Transform _player;
    private Vector2 _attackDirection;
    
    private enum AttackPhase
    {
        Teleporting,
        SpawningBats
    }
    
    private AttackPhase _attackPhase;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        _player = ServiceLocator.Get<PlayerScript>().transform;
    }

    private void OnEnable()
    {
        // Send a ray down to find ground level
        var hit = Physics2D.Raycast(transform.position, Vector2.down, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find ground"); return;}
        
        var ground = hit.point;
        ground.y += Boss.bossCollider.bounds.extents.y + Mathf.Abs(Boss.bossCollider.offset.y);
        
        // Find the left and right wall then pick a random spot between them
        hit = Physics2D.Raycast(ground, Vector2.right, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find right wall"); return;}
        
        var rightWall = hit.point;
        rightWall.x -= 1f;
        
        hit = Physics2D.Raycast(ground, Vector2.left, 100, LayerMask.GetMask("Ground"));
        if (hit.collider == null){Debug.LogError("Couldn't find left wall"); return;}
        
        var leftWall = hit.point;
        leftWall.x += 1f;
        
        var randomX = Random.Range(leftWall.x, rightWall.x);
        _teleportLocation = new Vector2(randomX, ground.y);
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
            var bat = DraculaReference.Pooler.GetPooledObject();
            bat.transform.position = position;
            bat.GetComponent<DraculaBatScript>().targetVector = _attackDirection;
            bat.SetActive(true);
        }
    }
}
