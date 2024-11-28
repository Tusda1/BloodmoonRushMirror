using UnityEngine;

public class SpitterEnemy : ReactiveBase
{
    [SerializeField] private float detectionDistance = 20f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileCooldown = 2f;
    
    private GameObject _projectilePrefab;
    private Transform _player;
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
    
    protected override void Scan()
    {
        // Use overlap circle to detect the player
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionDistance, LayerMask.GetMask("Player"));
        if (player != null)
        {
            // Set the direction to the player
            currentState = EnemyState.Action;
            _player = player.transform;
        }
    }
    
    protected override void Action()
    {
        // if distance to player is greater than detection distance, return to scanning
        if (Vector2.Distance(transform.position, _player.position) > detectionDistance)
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
            Attack();
        }
        else
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void Attack()
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
