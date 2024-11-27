using UnityEngine;

public class BatAttackState : BossState
{
    [SerializeField] private float attackLength = 0.5f;
    
    private GameObject _batPrefab;
    private Collider2D _collider;
    private float _timeSinceAttackStart;
    private float _timeSinceLastBatSpawn;
    private Vector2 _teleportLocation;
    private Pooler _pooler;
    private Teleport _teleport;
    
    private enum AttackPhase
    {
        Teleporting,
        SpawningBats
    }
    
    private AttackPhase _attackPhase;
    
    private void OnEnable()
    {
        _batPrefab = Resources.Load<GameObject>("Prefabs/DraculaBat");
        _pooler = new Pooler(_batPrefab, 10);
        _teleport = new Teleport(transform, Boss.AnimancerComponent, Boss.TeleportEnter, Boss.TeleportExit, OnTeleportEnd);
        
        _collider = GetComponent<Collider2D>();
        _timeSinceAttackStart = Time.time;
        
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
                if(!_teleport.IsTeleporting) _teleport.TeleportToLocation(_teleportLocation);
                break;
            case AttackPhase.SpawningBats:
                Bats();
                break;
        }
    }
    
    private void OnTeleportEnd()
    {
        _attackPhase = AttackPhase.SpawningBats;
        _timeSinceAttackStart = Time.time;
    }

    private void Bats()
    {
        // If the attack has been going on for 5 seconds, switch to the next state
        if (Time.time - _timeSinceAttackStart > attackLength)
        {
            return;
        }
        
        // Spawn a bat every 0.5 seconds randomly on the left side of the collider
        if (Time.time - _timeSinceLastBatSpawn > 0.1f)
        {
            _timeSinceLastBatSpawn = Time.time;
            var position = _collider.bounds.min;
            position.y += Random.Range(0, _collider.bounds.size.y);
            var bat = _pooler.GetPooledObject();
            bat.transform.position = position;
            bat.SetActive(true);
        }
    }
}
