using System;
using Player;
using UnityEngine;

public class MachineGunState : DrucillaState
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float shootCooldown = 0.1f;
    [SerializeField] private int projectileCount = 30;
    
    private Vector2 _teleportPosition;
    private float _shootTimer;
    private int _bulletCount;
    private PlayerScript _player;
    
    private Pooler _pooler;
    private CornerFinder _cornerFinder;
    
    private enum MachineGunStage
    {
        Teleport,
        Shoot
    }
    
    private MachineGunStage _stage;

    protected override void Awake()
    {
        base.Awake();
        
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }

    private void Start()
    {
        _player = ServiceLocator.Get<PlayerScript>();
        _pooler = TryGetCommunityPooler.TryGetPooler(projectilePrefab);
    }

    private void OnEnable()
    {
        var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);
        rightCorner.x -= 1;
        rightCorner.y -= 4;
        var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
        leftCorner.x += 1;
        leftCorner.y -= 4;
        
        var middle = new Vector2((leftCorner.x + rightCorner.x) / 2, leftCorner.y); // Find top middle of room
        _teleportPosition = middle;
    }

    private void OnDisable()
    {
        _bulletCount = 0;
        _shootTimer = 0;
    }

    private void Update()
    {
        switch (_stage)
        {
            case MachineGunStage.Teleport:
                if (!teleport.IsTeleporting) teleport.TeleportToLocation(_teleportPosition);
                break;
            case MachineGunStage.Shoot:
                Firing();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void OnTeleportEnd()
    {
        _stage = MachineGunStage.Shoot;
    }

    private void Firing()
    {
        if (_shootTimer > 0)
        {
            _shootTimer -= Time.deltaTime;
        }
        else
        {
            _bulletCount++;
            Shoot();
            _shootTimer = shootCooldown;
        }
        
        if (_bulletCount >= projectileCount)
        {
            EndState();
        }
    }
    
    private void Shoot()
    {
        var position = transform.position;
        var direction = (_player.transform.position - position).normalized;
        var projectile = _pooler.GetPooledObject();
        projectile.transform.position = position;
        projectile.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        projectile.SetActive(true);
    }
}
