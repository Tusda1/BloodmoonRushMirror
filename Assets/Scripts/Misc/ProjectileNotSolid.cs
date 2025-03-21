using UnityEngine;
using Animancer;

public class ProjectileNotSolid : GravityObject
{
    [SerializeField] private DamageInstance damageInstance;
    [SerializeField] private float moveSpeed = 5f;

    [HideInInspector] public Vector2 targetVector = Vector2.zero;

    private AnimancerComponent _animancerComponent;
    
    protected new void Awake()
    {
        base.Awake();
        
        GravityModifier = 0;
        _animancerComponent = GetComponent<AnimancerComponent>();
    }

    private void OnEnable()
    {
        if (targetVector == Vector2.zero)
        {
            Debug.LogError("Target vector is zero");
            gameObject.SetActive(false);
            return;
        }
        
        // Rotate the projectile to face the direction it is moving
        var angle = Mathf.Atan2(targetVector.y, targetVector.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void OnDisable()
    {
        targetVector = Vector2.zero;
    }
    
    protected override void ComputeVelocity()
    {
        targetVelocity = targetVector.normalized * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(damageInstance);
        }
        else if (other.CompareTag("ground"))
        {
            gameObject.SetActive(false);
        }
    }
}
