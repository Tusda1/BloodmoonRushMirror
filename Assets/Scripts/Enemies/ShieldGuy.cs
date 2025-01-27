using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class ShieldGuy : Skeleton
{
    [SerializeField] private Shield shield = new();
    
    protected override bool CanBeDamaged(DamageInstance damageInstance)
    {
        shield.ShieldBlock(base.Attack, player, damageInstance);
        
        return damageInstance.which == DamageInstance.WhichAttack.CrouchAttack;
    }
}
