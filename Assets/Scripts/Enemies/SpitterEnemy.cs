using UnityEngine;

public class SpitterEnemy : ReactiveBase
{
    [SerializeField] private float detectionDistance = 20f;
    [SerializeField] private float projectileSpeed = 10f;
    
    private GameObject _projectilePrefab;
    private Transform _player;
    
    protected new void Start()
    {
        base.Start();
        
        _projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");
    }
    
    protected override void Scan()
    {
        // Use overlap circle to detect the player
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionDistance, LayerMask.GetMask("Player"));
        if (player != null)
        {
            // Set the direction to the player
            facingDirection = (player.transform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
            currentState = EnemyState.Moving;
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
        
        // Shoot a projectile in the facingdirection. Spawn it so that it is outside of the collider of the spitter
        GameObject projectile = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody2D>().linearVelocity = facingDirection * projectileSpeed;
        projectile.GetComponent<Projectile>().shooter = gameObject;
    }
}
