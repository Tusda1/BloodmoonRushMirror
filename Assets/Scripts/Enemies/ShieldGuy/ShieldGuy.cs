using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class ShieldGuy : ReactiveBase
{
    [SerializeField] private Shield shield = new();
    [SerializeField] private Collider2D attackRange;
    [SerializeField] private float rangeBeforeStrike = 1.5f;
    [SerializeField] private float waitBeforeStrike = 2.5f;
    [SerializeField] private float distanceToCancelStrike = 3f;
    [SerializeField] private float keepAgroDistance = 2f;
    [SerializeField] private float stunnedTime = 1.7f;
    [SerializeField] private SmoothDampScript smoothDampScript = new();

    private float _timer;
    private bool _waitingForStrike;
    private Coroutine _strikeCoroutine;
    
    private ShieldGuyAnimation _shieldGuyAnimation;
    private ParticleSystem _particleSystem;

    protected new void Awake()
    {
        base.Awake();
        _shieldGuyAnimation = GetComponent<ShieldGuyAnimation>();
        _particleSystem = GetComponent<ParticleSystem>();
        shield.AttackBlocked += _shieldGuyAnimation.Block;
        shield.AttackNotBlocked += _shieldGuyAnimation.StunHit;
        shield.InitializeShield(Strike, this);
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
        if (_shieldGuyAnimation.isExecutingAnAction || isStunned || isKnockedUp || isDead) return;

        var distance = Vector2.Distance(targetObject.position, transform.position);
        
        // Check if the target is still in range
        if (distance > keepAgroDistance)
        {
            currentState = EnemyState.Scanning;
            _shieldGuyAnimation.PlayDefaultAnimation();
            return;
        }

        if (_waitingForStrike && distance > distanceToCancelStrike)
        {
            CancelWaitThenStrike();
        }
        
        // Check if the target is to the left or right
        facingDirection = (targetObject.position.x > transform.position.x) ? Vector2.right : Vector2.left;
        
        if (_waitingForStrike) return;
        
        RaycastHit2D foundPlayer = Physics2D.Raycast(GetBottomPosition(), facingDirection, rangeBeforeStrike, LayerMask.GetMask("Player"));
        if (foundPlayer.collider)
        {
            _shieldGuyAnimation.PlayDefaultAnimation();
            _strikeCoroutine = StartCoroutine(WaitThenStrike());
            return;
        }
        
        //Check if there is something in the way
        RaycastHit2D hit = Physics2D.Raycast(GetBottomPosition(), facingDirection, 1f, LayerMask.GetMask("Ground"));
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
    }

    protected override bool CanBeDamaged(DamageInstance damageInstance)
    {
        if (isStunned) return true;
        
        var blocked = shield.ShieldBlock(player, damageInstance, () => StartCoroutine(ShieldBreakCoroutine()));
        return !blocked;
    }

    protected override void OnTakeDamage(DamageInstance damageInstance)
    {
        if (isStunned) _shieldGuyAnimation.Stun();
    }

    private IEnumerator ShieldBreakCoroutine()
    {
        _shieldGuyAnimation.Stun();
        CancelWaitThenStrike();
        isVulnerable = true;
        isStunned = true;
        _particleSystem.Play();
        yield return new WaitForSeconds(stunnedTime);
        isVulnerable = false;
        isStunned = false;
        
        if (!isDead) Strike();
    }
    
    private IEnumerator WaitThenStrike()
    {
        _waitingForStrike = true;
        yield return new WaitForSeconds(waitBeforeStrike);
        Strike();
        _waitingForStrike = false;
    }
    
    private void CancelWaitThenStrike()
    {
        _waitingForStrike = false;
        if (_strikeCoroutine != null)
        {
            StopCoroutine(_strikeCoroutine);
            _strikeCoroutine = null;
        }
    }

    private void Strike()
    {
        _shieldGuyAnimation.Attack();
        // Use a overlayBox to check if the player is in the attack range
        var bounds = attackRange.bounds;
        Collider2D playerObject = Physics2D.OverlapBox(bounds.center, bounds.size, 0, LayerMask.GetMask("Player"));
        if (playerObject)
        {
            Attack();
        }
    }

    protected override void Die()
    {
        base.Die();
        
        _particleSystem.Stop();
        _particleSystem.Clear();
    }
}
