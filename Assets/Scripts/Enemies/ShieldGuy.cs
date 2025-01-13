using Player;
using UnityEngine;

public class ShieldGuy : Skeleton
{
    private int _blockCounter = 0;
    private GravityObjectPlayerMovement _playerMovement;
    
    protected override bool CanBeDamaged(DamageInstance damageInstance)
    {
        if (_playerMovement == null) _playerMovement = targetObject.GetComponent<GravityObjectPlayerMovement>();
        
        if (damageInstance.which != DamageInstance.WhichAttack.CrouchAttack)
        {
            _blockCounter++;

            if (damageInstance.which == DamageInstance.WhichAttack.DashAttack)
            {
                _playerMovement.isDashing = false;
                _blockCounter = 0;
                base.Attack();
                return true;
            }
            
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
