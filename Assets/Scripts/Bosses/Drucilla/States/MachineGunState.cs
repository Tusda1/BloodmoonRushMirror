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
    private PlayerScript _player;
    
    private Pooler _pooler;
    private CornerFinder _cornerFinder;
    
    private enum MachineGunStage
    {
        Teleport,
        Shoot
    }
    
    private MachineGunStage _stage;

    private void Start()
    {
        _player = ServiceLocator.Get<PlayerScript>();
        _pooler = TryGetCommunityPooler.TryGetPooler(projectilePrefab);
        _cornerFinder = new CornerFinder(transform, LayerMask.GetMask("Ground"));
    }

    private void OnEnable()
    {
        var rightCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopRight);
        rightCorner.x -= 1;
        rightCorner.y -= 2;
        var leftCorner = _cornerFinder.GetCorner(CornerFinder.CornerType.TopLeft);
        leftCorner.x += 1;
        leftCorner.y -= 2;
        
        var middle = (leftCorner + rightCorner) / 2; // Find top middle of room
        _teleportPosition = middle;
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
    
    private void Firing()
    {
        if (_shootTimer > 0)
        {
            _shootTimer -= Time.deltaTime;
        }
        else
        {
            Shoot();
            _shootTimer = shootCooldown;
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
