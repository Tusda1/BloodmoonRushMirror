using System;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Shield
{
    [SerializeField] private int blocksUntilCounterAttack = 3;
    [SerializeField] private bool alwaysCounterAttackOnDash = true;
    private int _blockCounter;
    private GravityObjectPlayerMovement _playerMovement;
    
    public void ShieldBlock(Action counterAttack, PlayerScript player, DamageInstance damageInstance)
    {
        if (!_playerMovement) _playerMovement = player.GetComponent<GravityObjectPlayerMovement>();
        
        if (damageInstance.which != DamageInstance.WhichAttack.CrouchAttack)
        {
            _blockCounter++;

            if (damageInstance.which == DamageInstance.WhichAttack.DashAttack)
            {
                _playerMovement.isDashing = false;
                if (alwaysCounterAttackOnDash)
                {
                    counterAttack();
                    _blockCounter = 0;
                }
                else
                {
                    _blockCounter++;
                }
            }
            
            else if (_blockCounter >= blocksUntilCounterAttack)
            {
                _blockCounter = 0;
                counterAttack();
            }
        }
    }
}
