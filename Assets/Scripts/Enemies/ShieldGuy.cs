using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class ShieldGuy : Skeleton
{
    [SerializeField] private Shield shield = new();
    [SerializeField] private Collider2D attackRange;
    [SerializeField] private float waitBeforeStrike = 2.5f;

    private float _timer;

    protected override void Action()
    {
        base.Action();
        
        if (isStunned || isKnockedUp || isDead) return;
        
        // Check if player is in attack range and if he is there for a certain amount of time then strike
        var bounds = attackRange.bounds;
        Collider2D playerObject = Physics2D.OverlapBox(bounds.center, bounds.size, 0, LayerMask.GetMask("Player"));
        if (playerObject)
        {
            if (_timer > waitBeforeStrike)
            {
                _timer = 0;
                Attack();
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

    protected override bool CanBeDamaged(DamageInstance damageInstance)
    {
        if (isStunned) return true;
        
        return !shield.ShieldBlock(Attack, player, damageInstance, () => StartCoroutine(ShieldBreakCoroutine()));
    }
    
    private IEnumerator ShieldBreakCoroutine()
    {
        isVulnerable = true;
        isStunned = true;
        yield return new WaitForSeconds(2);
        isVulnerable = false;
        isStunned = false;
        Attack();
    }

    protected override void Attack()
    {
        // Use a overlayBox to check if the player is in the attack range
        var bounds = attackRange.bounds;
        Collider2D playerObject = Physics2D.OverlapBox(bounds.center, bounds.size, 0, LayerMask.GetMask("Player"));
        if (playerObject)
        {
            base.Attack();
        }
    }
}
