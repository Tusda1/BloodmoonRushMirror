using Player;
using UnityEngine;

public class ShieldGuy : Skeleton
{
    private int _blockCounter = 0;
    private Shield _shield = new();
    
    protected override bool CanBeDamaged(DamageInstance damageInstance)
    {
        _shield.ShieldBlock(base.Attack, player, damageInstance);
        
        return damageInstance.which == DamageInstance.WhichAttack.CrouchAttack;
    }
}
