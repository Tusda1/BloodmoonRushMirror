using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class ShieldGuy : ReactiveBase
{
    [SerializeField] private Shield shield = new();
    [SerializeField] private Collider2D attackRange;
    [SerializeField] private float waitBeforeStrike = 2.5f;
    [SerializeField] private float keepAgroDistance = 2f;
    [SerializeField] private SmoothDampScript smoothDampScript = new();

    private float _timer;
    
    private ShieldGuyAnimation _shieldGuyAnimation;

    protected new void Awake()
    {
        base.Awake();
        _shieldGuyAnimation = GetComponent<ShieldGuyAnimation>();
        shield.AttackBlocked += _shieldGuyAnimation.Block;
        shield.AttackNotBlocked += _shieldGuyAnimation.StunHit;
    }

    protected override void Scan()
    {
        //Send a ray to the right
        RaycastHit2D hit = Physics2D.Raycast(GetBottomPosition() + Vector2.up, facingDirection, detectionDistance, LayerMask.GetMask("Player"));
        if (hit.collider != null)
        {
            currentState = EnemyState.Action;
            targetObject = hit.transform;
        }
        
        //Send a ray to the left
        hit = Physics2D.Raycast(GetBottomPosition() + Vector2.up, -facingDirection, detectionDistance, LayerMask.GetMask("Player"));
        if (hit.collider != null)
        {
            currentState = EnemyState.Action;
            targetObject = hit.transform;
        }
    }

    protected override void Action()
    {
        if (_shieldGuyAnimation.isExecutingAnAction) return;
        
        // Check if the target is still in range
        if (Vector2.Distance(targetObject.position, transform.position) > keepAgroDistance)
        {
            currentState = EnemyState.Scanning;
            _shieldGuyAnimation.PlayDefaultAnimation();
            return;
        }
        
        // Check if the target is to the left or right
        facingDirection = (targetObject.position.x > transform.position.x) ? Vector2.right : Vector2.left;
        
        //Check if there is something in the way
        RaycastHit2D hit = Physics2D.Raycast(GetBottomPosition(), facingDirection, 1f, LayerMask.GetMask("Player", "Ground"));
        if (hit.collider)
        {
            _shieldGuyAnimation.PlayDefaultAnimation();
        }
        else
        {
            // Smoothly move towards the target
            smoothDampScript.SmoothDampVector2(facingDirection * moveSpeed, ref targetVelocity);
            _shieldGuyAnimation.Walk();
        }
        
        if (isStunned || isKnockedUp || isDead) return;
        
        // Check if player is in attack range and if he is there for a certain amount of time then strike
        var bounds = attackRange.bounds;
        Collider2D playerObject = Physics2D.OverlapBox(bounds.center, bounds.size, 0, LayerMask.GetMask("Player"));
        if (playerObject)
        {
            if (_timer > waitBeforeStrike)
            {
                _timer = 0;
                Strike();
            }
            else
            {
                _timer += Time.deltaTime;
            }
        }
        else
        {
            _timer = 0;
        }
    }

    protected override void SetVisualFacingDirection()
    {
        visualFacingDirection = -facingDirection;
    }

    protected override bool CanBeDamaged(DamageInstance damageInstance)
    {
        if (isStunned) return true;
        
        return !shield.ShieldBlock(Strike, player, damageInstance, () => StartCoroutine(ShieldBreakCoroutine()));
    }

    protected override void OnTakeDamage(DamageInstance damageInstance)
    {
        _shieldGuyAnimation.Stun();
    }

    private IEnumerator ShieldBreakCoroutine()
    {
        isVulnerable = true;
        isStunned = true;
        yield return new WaitForSeconds(2);
        isVulnerable = false;
        isStunned = false;
        Strike();
    }

    private void Strike()
    {
        // Use a overlayBox to check if the player is in the attack range
        var bounds = attackRange.bounds;
        Collider2D playerObject = Physics2D.OverlapBox(bounds.center, bounds.size, 0, LayerMask.GetMask("Player"));
        if (playerObject)
        {
            _shieldGuyAnimation.Attack();
            Attack();
        }
    }
}
