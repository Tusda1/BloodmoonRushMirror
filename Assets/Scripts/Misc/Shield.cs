using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Shield
{
    [SerializeField] private int blocksUntilCounterAttack = 3;
    [SerializeField] private bool alwaysCounterAttackOnDash = true;
    [SerializeField] private int blowsUntilBreak = 2;
    [SerializeField] private float breakResetTime = 5f;
    private int _blockCounter;
    private float _lastHitTime;
    private int _breakCounter;
    private GravityObjectPlayerMovement _playerMovement;
    
    public bool ShieldBlock(Action counterAttack, PlayerScript player, DamageInstance damageInstance, Action shieldBreakAction = null)
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

            return true;
        }
        
        if (Time.time - _lastHitTime > breakResetTime)
        {
            _breakCounter = 0;
        }
        
        _breakCounter++;
        if (_breakCounter >= blowsUntilBreak)
        {
            shieldBreakAction?.Invoke();
            _lastHitTime = Time.time;
            _breakCounter = 0;
        }
        
        return false;
    }
}
