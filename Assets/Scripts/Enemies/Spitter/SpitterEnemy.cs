using System.Linq;
using UnityEngine;

public class SpitterEnemy : ReactiveBase
{
    [SerializeField] private float projectileCooldown = 2f;
    
    private GameObject _projectilePrefab;
    private SpitterAnimation _spitterAnimation;
    private Pooler _projectilePooler;
    private bool _attacking;
    private float _cooldownTimer;
    
    private const float YOffset = 0.45f;

    protected override void Start()
    {
        base.Start();
        
        _projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");
        _spitterAnimation = enemyAnimationBase as SpitterAnimation;
        
        try
        {
            var communityPooler = ServiceLocator.Get<CommunityPooler>();
            foreach (var pooler in communityPooler.poolers.Where(pooler => pooler.ObjectToPool == _projectilePrefab))
            {
                _projectilePooler = pooler;
                break;
            }

            if (_projectilePooler == null)
            {
                Debug.LogWarning("No pooler found for projectile, creating new pooler");
                _projectilePooler = new Pooler(_projectilePrefab, 5);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to retrieve CommunityPooler: {ex.Message}");
            _projectilePooler = new Pooler(_projectilePrefab, 5);
        }
    }
    
    protected override void Action()
    {
        // if distance to player is greater than detection distance, return to scanning
        if (Vector2.Distance(transform.position, targetObject.position) > detectionDistance)
        {
            currentState = EnemyState.Scanning;
            return;
        }

        if (_spitterAnimation.isAttacking)
        {
            _cooldownTimer = projectileCooldown;
            return;
        }
        
        // check if the cooldown has passed
        if (_cooldownTimer <= 0)
        {
            StartAttack();
        }
        else
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    private void StartAttack()
    {
        _spitterAnimation.Attack(ShootProjectile);
    }
    
    private void ShootProjectile()
    {
        print("shooting projectile");
        var position = transform.position;
        var spawnPosition = new Vector2(position.x, position.y + YOffset);
        
        // Shoot a projectile in the facingdirection. Spawn it so that it is outside of the collider of the spitter
        GameObject projectile = _projectilePooler.GetPooledObject();
        projectile.transform.position = spawnPosition;
        var projectileScript = projectile.GetComponent<ProjectileDeflectable>();
        projectileScript.shooter = gameObject;
        projectileScript.velocityDirection = facingDirection;
        projectile.SetActive(true);
    }
}
