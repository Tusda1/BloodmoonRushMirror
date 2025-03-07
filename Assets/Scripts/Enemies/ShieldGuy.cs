using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class ShieldGuy : Skeleton
{
    [SerializeField] private Shield shield = new();
    [SerializeField] private Collider2D attackRange;
    
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
