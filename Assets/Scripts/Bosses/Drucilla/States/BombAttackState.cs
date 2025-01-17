using System;
using Player;
using UnityEngine;

public class BombAttackState : DrucillaState
{
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private float bombCooldown = 1f;
    [SerializeField] private float glideSpeed = 4f;
    [SerializeField] private float glideFallVelocity = -1f;
    
    private float _bombTimer;
    private Vector2 _teleportPosition;
    
    private PlayerScript _player;
    private Pooler _bombPooler;
    private SmoothDampScript _smoothDampScript = new();
    
    private enum BombAttackStage
    {
        Teleport,
        DropBombs,
        GroundSlam
    }
    
    private BombAttackStage _stage;

    private void Start()
    {
        _player = ServiceLocator.Get<PlayerScript>();
        
        _bombPooler = TryGetCommunityPooler.TryGetPooler(bombPrefab);
    }

    private void OnEnable()
    {
        Boss.GravityModifier = 0;
        
        var hit = Physics2D.Raycast(_player.transform.position, Vector2.up, 100f, LayerMask.GetMask("Ground"));
        
        _teleportPosition = hit.point + Vector2.down * 2.5f;
    }

    private void Update()
    {
        switch (_stage)
        {
            case BombAttackStage.Teleport:
                if (!teleport.IsTeleporting) teleport.TeleportToLocation(_teleportPosition);
                break;
            case BombAttackStage.DropBombs:
                DropBombs();
                break;
            case BombAttackStage.GroundSlam:
                GroundSlam();
                break;
        }
    }

    protected override void OnTeleportEnd()
    {
        _stage = BombAttackStage.DropBombs;
    }

    private void DropBombs()
    {
        if (_bombTimer > bombCooldown)
        {
            _bombTimer += Time.deltaTime;
        }
        else
        {
            _bombTimer = 0;
            SpawnBomb();
        }
        
        Vector2 vector = Vector2.zero;
        
        Vector2 direction = (_player.transform.position - transform.position).normalized.x > 0 ? Vector2.right : Vector2.left;
        
        _smoothDampScript.SmoothDampFloat(direction, glideSpeed, ref vector);
        
        Boss.targetVelocityX = vector.x;
        Boss.velocityY = glideFallVelocity;
    }
    
    private void SpawnBomb()
    {
        var bomb = _bombPooler.GetPooledObject();
        bomb.transform.position = transform.position;
        bomb.SetActive(true);
    }
    
    private void GroundSlam()
    {
        
    }
}
