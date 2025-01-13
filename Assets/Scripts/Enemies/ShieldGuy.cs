using UnityEngine;

public class ShieldGuy : Skeleton
{
    private int _blockCounter = 0;
    
    protected override bool CanBeDamaged(DamageInstance damageInstance)
    {
        if (damageInstance.which != DamageInstance.WhichAttack.CrouchAttack)
        {
            _blockCounter++;
            
            if (_blockCounter >= 3)
            {
                _blockCounter = 0;
                base.Attack();
                return true;
            }
        }
        
        return damageInstance.which == DamageInstance.WhichAttack.CrouchAttack;
    }
}
