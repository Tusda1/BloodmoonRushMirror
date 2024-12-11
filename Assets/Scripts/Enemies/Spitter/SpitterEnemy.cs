using UnityEngine;

public class SpitterEnemy : ReactiveBase
{
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileCooldown = 2f;
    
    private GameObject _projectilePrefab;
    private SpitterAnimation _spitterAnimation;
    private bool _attacking;
    private float cooldownTimer;
    
    private const float YOffset = 0.45f;

    protected new void Start()
    {
        base.Start();
        
        _projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");
        _spitterAnimation = enemyAnimationBase as SpitterAnimation;
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
            cooldownTimer = projectileCooldown;
            return;
        }
        
        // check if the cooldown has passed
        if (cooldownTimer <= 0)
        {
            StartAttack();
        }
        else
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void StartAttack()
    {
        _spitterAnimation.Attack(ShootProjectile);
    }
    
    private void ShootProjectile()
    {
        var position = transform.position;
        var spawnPosition = new Vector2(position.x, position.y + YOffset);
        
        // Shoot a projectile in the facingdirection. Spawn it so that it is outside of the collider of the spitter
        GameObject projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
        projectile.GetComponent<Rigidbody2D>().linearVelocity = facingDirection * projectileSpeed;
        projectile.GetComponent<ProjectileDeflectable>().shooter = gameObject;
    }
}
