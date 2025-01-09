using Player;
using UnityEngine;

public class Gargoyle : ReactiveBase
{
    [SerializeField] private float projectileCooldown = 2f;
    private float _cooldownTimer;
    
    private const float YOffset = 0.45f;
    private GameObject _projectilePrefab;
    private Pooler _projectilePooler;
    private Transform _playerTransform;
    
    protected override void Start()
    {
        base.Start();
        
        _projectilePrefab = Resources.Load<GameObject>("Prefabs/GargoyleProjectile");
        _playerTransform = ServiceLocator.Get<PlayerScript>().transform;
        _projectilePooler = TryGetCommunityPooler.TryGetPooler(_projectilePrefab);
    }

    protected override void Action()
    {
        // if distance to player is greater than detection distance, return to scanning
        if (Vector2.Distance(transform.position, targetObject.position) > detectionDistance)
        {
            currentState = EnemyState.Scanning;
            return;
        }

        // check if the cooldown has passed
        if (_cooldownTimer <= 0)
        {
            StartAttack();
            _cooldownTimer = projectileCooldown;
        }
        else
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }
    
    private void StartAttack()
    {
        var position = transform.position;
        var spawnPosition = new Vector2(position.x, position.y + YOffset);
        
        // Shoot a projectile in the facingdirection. Spawn it so that it is outside of the collider of the spitter
        GameObject projectile = _projectilePooler.GetPooledObject();
        projectile.transform.position = spawnPosition;
        var projectileScript = projectile.GetComponent<ProjectileDeflectable>();
        projectileScript.shooter = gameObject;
        projectileScript.velocityDirection = (_playerTransform.position - position).normalized;
        projectile.SetActive(true);
    }
}
